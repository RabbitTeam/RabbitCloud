using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Facade.Abstractions;

namespace Rabbit.Cloud.Facade.Internal
{
    internal class FacadeBuilder : IFacadeBuilder
    {
        public FacadeBuilder(IServiceCollection services)
        {
            Services = services;
        }

        #region Implementation of IFacadeBuilder

        public IServiceCollection Services { get; }

        #endregion Implementation of IFacadeBuilder
    }
}