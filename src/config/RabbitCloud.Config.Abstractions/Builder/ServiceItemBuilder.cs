using System;

namespace RabbitCloud.Config.Abstractions.Builder
{
    public class ServiceItemBuilder
    {
        internal readonly ServiceConfigModel Model = new ServiceConfigModel();

        public ServiceItemBuilder Key(string key)
        {
            Model.ServiceKey = key;
            return this;
        }

        public ServiceItemBuilder Type(Type type)
        {
            Model.ServiceType = type;
            return this;
        }

        public ServiceItemBuilder Factory(Func<object> factory)
        {
            Model.ServiceFactory = factory;
            return this;
        }

        public ServiceConfigModel Build()
        {
            return Model;
        }
    }

    public static class ServiceItemBuilderExtensions
    {
        public static ServiceItemBuilder Type<T>(this ServiceItemBuilder builder)
        {
            return builder.Type(typeof(T));
        }

        public static ServiceItemBuilder Factory<T>(this ServiceItemBuilder builder, Func<T> factory)
        {
            if (string.IsNullOrEmpty(builder.Model.ServiceKey))
                builder.Key(typeof(T).Name);

            return builder
                .Type<T>()
                .Factory(() => factory());
        }
    }
}