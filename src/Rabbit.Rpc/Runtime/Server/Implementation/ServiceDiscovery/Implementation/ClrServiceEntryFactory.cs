using Rabbit.Rpc.Ids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// Clr服务条目工厂。
    /// </summary>
    public class ClrServiceEntryFactory : IClrServiceEntryFactory
    {
        #region Field

        private readonly IServiceInstanceFactory _serviceFactory;
        private readonly IServiceIdGenerator _serviceIdGenerator;

        #endregion Field

        #region Constructor

        public ClrServiceEntryFactory(IServiceInstanceFactory serviceFactory, IServiceIdGenerator serviceIdGenerator)
        {
            _serviceFactory = serviceFactory;
            _serviceIdGenerator = serviceIdGenerator;
        }

        #endregion Constructor

        #region Implementation of IClrServiceEntryFactory

        /// <summary>
        /// 创建服务条目。
        /// </summary>
        /// <param name="service">服务类型。</param>
        /// <param name="serviceImplementation">服务实现类型。</param>
        /// <returns>服务条目集合。</returns>
        public IEnumerable<ServiceEntry> CreateServiceEntry(Type service, Type serviceImplementation)
        {
            foreach (var methodInfo in service.GetTypeInfo().GetMethods())
            {
                var implementationMethodInfo = serviceImplementation.GetTypeInfo().GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                yield return Create(_serviceIdGenerator.GenerateServiceId(methodInfo), implementationMethodInfo);
            }
        }

        #endregion Implementation of IClrServiceEntryFactory

        #region Private Method

        private ServiceEntry Create(string serviceId, MethodBase implementationMethod)
        {
            var type = implementationMethod.DeclaringType;

            return new ServiceEntry
            {
                Descriptor = new ServiceDescriptor
                {
                    Id = serviceId
                },
                Func = parameters =>
                {
                    var instance = _serviceFactory.Create(type);

                    var list = new List<object>();
                    foreach (var parameterInfo in implementationMethod.GetParameters())
                    {
                        var value = parameters[parameterInfo.Name];

                        list.Add(value);
                    }

                    var result = implementationMethod.Invoke(instance, list.ToArray());

                    return result;
                }
            };
        }

        #endregion Private Method
    }
}