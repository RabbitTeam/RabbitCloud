using Rabbit.Cloud.Facade.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Internal
{
    public class FacadeApplicationBuilder : IFacadeApplicationBuilder
    {
        private readonly IList<Func<FacadeRequestDelegate, FacadeRequestDelegate>> _components = new List<Func<FacadeRequestDelegate, FacadeRequestDelegate>>();

        #region Constructor

        public FacadeApplicationBuilder(IServiceProvider serviceProvider)
        {
            Properties = new Dictionary<string, object>(StringComparer.Ordinal);
            ApplicationServices = serviceProvider;
        }

        private FacadeApplicationBuilder(IFacadeApplicationBuilder builder)
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

        public FacadeRequestDelegate Build()
        {
            FacadeRequestDelegate app = context =>
            {
                context.ResponseMessage.StatusCode = HttpStatusCode.NotFound;
                return Task.CompletedTask;
            };

            foreach (var component in _components.Reverse())
            {
                app = component(app);
            }

            return app;
        }

        public IFacadeApplicationBuilder New()
        {
            return new FacadeApplicationBuilder(this);
        }

        public IFacadeApplicationBuilder Use(Func<FacadeRequestDelegate, FacadeRequestDelegate> middleware)
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