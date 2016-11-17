using System;
using System.Collections.Generic;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class ServiceBuilder
    {
        private readonly List<ServiceConfigModel> _services = new List<ServiceConfigModel>();

        public ServiceBuilder AddServices(params ServiceConfigModel[] services)
        {
            _services.AddRange(services);
            return this;
        }

        public ServiceBuilder AddService(Action<ServiceItemBuilder> config)
        {
            var serviceBuilder = new ServiceItemBuilder();
            config(serviceBuilder);
            return AddServices(serviceBuilder.Build());
        }

        public IEnumerable<ServiceConfigModel> Build()
        {
            return _services;
        }
    }
}