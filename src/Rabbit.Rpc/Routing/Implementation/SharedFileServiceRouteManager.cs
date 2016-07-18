using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Routing.Implementation
{
    /// <summary>
    /// 基于共享文件的服务路由管理者。
    /// </summary>
    public class SharedFileServiceRouteManager : ServiceRouteManagerBase, IDisposable
    {
        #region Field

        private readonly string _filePath;
        private readonly ISerializer<string> _serializer;
        private readonly IServiceRouteFactory _serviceRouteFactory;
        private readonly ILogger<SharedFileServiceRouteManager> _logger;
        private IEnumerable<ServiceRoute> _routes;
        private readonly FileSystemWatcher _fileSystemWatcher;

        private DateTime _lastWriteTime;

        #endregion Field

        #region Constructor

        public SharedFileServiceRouteManager(string filePath, ISerializer<string> serializer,
            IServiceRouteFactory serviceRouteFactory, ILogger<SharedFileServiceRouteManager> logger) : base(serializer)
        {
            _filePath = filePath;
            _serializer = serializer;
            _serviceRouteFactory = serviceRouteFactory;
            _logger = logger;

            var directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryName))
                _fileSystemWatcher = new FileSystemWatcher(directoryName, "*" + Path.GetExtension(filePath));

            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.IncludeSubdirectories = false;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        #endregion Constructor

        #region Private Method

        private async Task<IEnumerable<ServiceRoute>> GetRoutes(string file)
        {
            ServiceRoute[] routes;
            if (File.Exists(file))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从文件：{file}中获取服务路由。");
                string content;
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var reader = new StreamReader(fileStream, Encoding.UTF8);
                    content = await reader.ReadToEndAsync();
                }
                try
                {
                    var serializer = _serializer;
                    routes =
                        (await
                            _serviceRouteFactory.CreateServiceRoutesAsync(
                                serializer.Deserialize<string, ServiceRouteDescriptor[]>(content))).ToArray();
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation(
                            $"成功获取到以下路由信息：{string.Join(",", routes.Select(i => i.ServiceDescriptor.Id))}。");
                }
                catch (Exception exception)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError("获取路由信息时发生了错误。", exception);
                    routes = new ServiceRoute[0];
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取路由信息，因为文件：{file}不存在。");
                routes = new ServiceRoute[0];
            }
            return routes;
        }

        private async Task EntryRoutes(string file)
        {
            var oldRoutes = _routes?.ToArray();
            var newRoutes = (await GetRoutes(file)).ToArray();
            if (oldRoutes == null)
            {
                _routes = newRoutes;
            }
            else
            {
                //旧的服务Id集合。
                var oldServiceIds = oldRoutes.Select(i => i.ServiceDescriptor.Id).ToArray();
                //新的服务Id集合。
                var newServiceIds = newRoutes.Select(i => i.ServiceDescriptor.Id).ToArray();

                //被删除的服务Id集合
                var removeServiceIds = oldServiceIds.Except(newServiceIds).ToArray();
                //新增的服务Id集合。
                var addServiceIds = newServiceIds.Except(oldServiceIds).ToArray();
                //可能被修改的服务Id集合。
                var mayModifyServiceIds = newServiceIds.Except(removeServiceIds).ToArray();

                //触发服务路由创建事件。
                foreach (var route in newRoutes.Where(i => addServiceIds.Contains(i.ServiceDescriptor.Id)))
                    OnCreated(new ServiceRouteEventArgs(route));

                //触发服务路由删除事件。
                foreach (var route in oldRoutes.Where(i => removeServiceIds.Contains(i.ServiceDescriptor.Id)))
                    OnRemoved(new ServiceRouteEventArgs(route));

                //触发服务路由变更事件。
                var currentMayModifyRoutes = newRoutes.Where(i => mayModifyServiceIds.Contains(i.ServiceDescriptor.Id)).ToArray();
                var oldMayModifyRoutes = oldRoutes.Where(i => mayModifyServiceIds.Contains(i.ServiceDescriptor.Id)).ToArray();

                foreach (var currentMayModifyRoute in currentMayModifyRoutes)
                {
                    if (!oldMayModifyRoutes.Contains(currentMayModifyRoute))
                        OnChanged(new ServiceRouteChangedEventArgs(currentMayModifyRoute, oldMayModifyRoutes.First(i => i.ServiceDescriptor.Id == currentMayModifyRoute.ServiceDescriptor.Id)));
                }
            }
        }

        private async void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            var newTime = File.GetLastWriteTime(e.FullPath);

            //防止事件重复执行。
            if (newTime <= _lastWriteTime)
                return;

            _lastWriteTime = newTime;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"文件{_filePath}发生了变更，将重新获取路由信息。");
            await EntryRoutes(_filePath);
        }

        #endregion Private Method

        #region Overrides of ServiceRouteManagerBase

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            if (_routes == null)
                await EntryRoutes(_filePath);
            return _routes;
        }

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override Task ClearAsync()
        {
            return Task.Run(() =>
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            });
        }

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        protected override async Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    File.WriteAllText(_filePath, _serializer.Serialize(routes), Encoding.UTF8);
                }
            });
        }

        #endregion Overrides of ServiceRouteManagerBase

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _fileSystemWatcher?.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}