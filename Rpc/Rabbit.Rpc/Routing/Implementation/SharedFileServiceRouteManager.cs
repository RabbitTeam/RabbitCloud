using Rabbit.Rpc.Address;
using Rabbit.Rpc.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Routing.Implementation
{
    /// <summary>
    /// 基于共享文件的服务路由管理者。
    /// </summary>
    public class SharedFileServiceRouteManager : IServiceRouteManager, IDisposable
    {
        #region Field

        private readonly string _filePath;
        private readonly ISerializer _serializer;
        private IEnumerable<ServiceRoute> _routes;
        private readonly FileSystemWatcher _fileSystemWatcher;

        #endregion Field

        #region Constructor

        public SharedFileServiceRouteManager(string filePath, ISerializer serializer)
        {
            _filePath = filePath;
            _serializer = serializer;
            _fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), "*" + Path.GetExtension(filePath));
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Changed;
            _fileSystemWatcher.IncludeSubdirectories = false;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            EntryRoutes(_filePath);
        }

        #endregion Constructor

        #region Implementation of IServiceRouteManager

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            if (_routes == null)
                EntryRoutes(_filePath);
            return Task.FromResult(_routes);
        }

        /// <summary>
        /// 添加服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        public async Task AddRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    File.WriteAllBytes(_filePath, _serializer.Serialize(routes));
                }
            });
        }

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public Task ClearAsync()
        {
            return Task.Run(() =>
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            });
        }

        #endregion Implementation of IServiceRouteManager

        #region Private Method

        private void EntryRoutes(string file)
        {
            lock (this)
            {
                if (File.Exists(file))
                {
                    var content = File.ReadAllBytes(file);
                    try
                    {
                        _routes = _serializer.Deserialize<IpAddressDescriptor[]>(content).Select(i => new ServiceRoute
                        {
                            Address = i.Address,
                            ServiceDescriptor = i.ServiceDescriptor
                        }).ToArray();
                    }
                    catch
                    {
                        _routes = Enumerable.Empty<ServiceRoute>();
                    }
                }
                else
                {
                    _routes = Enumerable.Empty<ServiceRoute>();
                }
            }
        }

        #endregion Private Method

        protected class IpAddressDescriptor
        {
            public List<IpAddressModel> Address { get; set; }
            public ServiceDescriptor ServiceDescriptor { get; set; }
        }

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }

        #endregion Implementation of IDisposable
    }
}