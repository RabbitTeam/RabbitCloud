using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Go.Abstractions;
using Rabbit.Go.ApplicationModels;
using Rabbit.Go.Internal;
using System.Linq;

namespace Rabbit.Go
{
    public class RabbitGoInterceptor : GoInterceptor
    {
        private readonly RabbitRequestDelegate _app;
        private readonly ApplicationModel _applicationModel;

        public RabbitGoInterceptor(RabbitRequestDelegate app, ApplicationModel applicationModel)
        {
            _app = app;
            _applicationModel = applicationModel;
        }

        #region Overrides of GoInterceptor

        protected override IGoRequestInvoker CreateServiceInvoker(InterceptContext interceptContext)
        {
            var proxyType = interceptContext.ProxyType;
            var method = interceptContext.Invocation.Method;

            var requestModel = _applicationModel.Services.SelectMany(s => s.Requests)
                .SingleOrDefault(i => i.ServiceModel.Type == proxyType && i.MethodInfo == method);

            return new RabbitGoInvoker(_app, new RequestContext(new RabbitContext(), interceptContext.Arguments), requestModel);
        }

        #endregion Overrides of GoInterceptor
    }
}