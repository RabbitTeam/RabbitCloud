using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class ContainerBuilder
    {
        private readonly ContainerConfigModel _model = new ContainerConfigModel();

        public ContainerBuilder UseAddress(Url url)
        {
            _model.Address = url;
            return this;
        }

        public ContainerBuilder ConfigurationServices(Action<ServiceBuilder> configurationServices)
        {
            var builder = new ServiceBuilder();
            configurationServices(builder);
            _model.ServiceConfigModels = builder.Build();

            return this;
        }

        public ContainerConfigModel Build()
        {
            return _model;
        }
    }
}