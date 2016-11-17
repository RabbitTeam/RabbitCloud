using System;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class ServiceBuilder
    {
        private readonly ServiceConfigModel _model = new ServiceConfigModel();

        public ServiceBuilder Id(string id)
        {
            _model.ServiceId = id;
            return this;
        }

        public ServiceBuilder Factory<T>(Func<T> factory)
        {
            _model.ServiceId = typeof(T).Name;
            _model.ServiceType = typeof(T);
            _model.ServiceFactory = () => factory();
            return this;
        }

        public ServiceBuilder Factory<T>(T instance)
        {
            return Factory(() => instance);
        }

        public ServiceConfigModel Build()
        {
            return _model;
        }
    }
}