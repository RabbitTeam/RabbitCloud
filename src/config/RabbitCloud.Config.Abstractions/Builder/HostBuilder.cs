using System;
using System.Linq;
using System.Net;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class HostBuilder
    {
        private readonly HostConfigModel _model = new HostConfigModel();

        public HostBuilder SetProtocol(string protocol)
        {
            _model.Protocol = protocol;
            return this;
        }

        public HostBuilder SetAddress(EndPoint address)
        {
            _model.Address = address;
            return this;
        }

        public HostBuilder SetAddress(string ip, int port)
        {
            return SetAddress(new IPEndPoint(IPAddress.Parse(ip), port));
        }
        
        public HostBuilder AddServices(params ServiceConfigModel[] services)
        {
            _model.ServiceConfigModels = _model.ServiceConfigModels == null ? services : _model.ServiceConfigModels.Concat(services);
            return this;
        }

        public HostBuilder AddService(Action<ServiceBuilder> config)
        {
            var serviceBuilder = new ServiceBuilder();
            config(serviceBuilder);
            return AddServices(serviceBuilder.Build());
        }

        public HostConfigModel Build()
        {
            return _model;
        }
    }
}