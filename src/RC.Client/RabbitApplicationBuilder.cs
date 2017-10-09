using Rabbit.Cloud.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class RabbitApplicationBuilder : IRabbitApplicationBuilder
    {
        private readonly IList<Func<RabbitRequestDelegate, RabbitRequestDelegate>> _components = new List<Func<RabbitRequestDelegate, RabbitRequestDelegate>>();

        #region Constructor

        public RabbitApplicationBuilder(IServiceProvider serviceProvider)
        {
            Properties = new Dictionary<string, object>(StringComparer.Ordinal);
            ApplicationServices = serviceProvider;
        }

        private RabbitApplicationBuilder(IRabbitApplicationBuilder builder)
        {
            Properties = new Dictionary<string, object>(builder.Properties, StringComparer.Ordinal);
        }

        #endregion Constructor

        #region Implementation of IFacadeApplicationBuilder

        public IServiceProvider ApplicationServices
        {
            get => GetProperty<IServiceProvider>(Constants.BuilderProperties.ApplicationServices);
            set => SetProperty(Constants.BuilderProperties.ApplicationServices, value);
        }

        public IDictionary<string, object> Properties { get; }

        public RabbitRequestDelegate Build()
        {
            RabbitRequestDelegate app = context => Task.CompletedTask;

            foreach (var component in _components.Reverse())
            {
                app = component(app);
            }

            return app;
        }

        public IRabbitApplicationBuilder New()
        {
            return new RabbitApplicationBuilder(this);
        }

        public IRabbitApplicationBuilder Use(Func<RabbitRequestDelegate, RabbitRequestDelegate> middleware)
        {
            _components.Add(middleware);
            return this;
        }

        #endregion Implementation of IFacadeApplicationBuilder

        #region Private Method

        private T GetProperty<T>(string key)
        {
            return Properties.TryGetValue(key, out var value) ? (T)value : default(T);
        }

        private void SetProperty<T>(string key, T value)
        {
            Properties[key] = value;
        }

        #endregion Private Method
    }
}