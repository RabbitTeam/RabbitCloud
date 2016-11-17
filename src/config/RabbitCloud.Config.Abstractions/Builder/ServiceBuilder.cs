using System;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class ServiceBuilder : Builder
    {
        private Func<object> _serviceFactory;

        public ServiceBuilder Factory(Func<object> factory)
        {
            _serviceFactory = factory;
            return this;
        }

        public Func<object> Build()
        {
            return _serviceFactory;
        }
    }
}