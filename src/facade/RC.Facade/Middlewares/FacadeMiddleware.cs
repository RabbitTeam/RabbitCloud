using Microsoft.AspNetCore.Routing;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Features;
using Rabbit.Cloud.Facade.Internal;
using Rabbit.Cloud.Facade.Utilities.Extensions;
using RC.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Middlewares
{
    public class FacadeMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IRequestMessageBuilder _requestMessageBuilder;
        private readonly IServiceDescriptorCollectionProvider _serviceDescriptorCollectionProvider;

        public FacadeMiddleware(RabbitRequestDelegate next, IRequestMessageBuilder requestMessageBuilder, IServiceDescriptorCollectionProvider serviceDescriptorCollectionProvider)
        {
            _next = next;
            _requestMessageBuilder = requestMessageBuilder;
            _serviceDescriptorCollectionProvider = serviceDescriptorCollectionProvider;
        }

        public async Task Invoke(RabbitContext context)
        {
            var invocationFeature = context.Features.Get<IInvocationFeature>();
            var invocation = invocationFeature.Invocation;

            var serviceDescriptor =
                _serviceDescriptorCollectionProvider.ServiceDescriptors.GetServiceDescriptor(invocation.Method
                    .GetHashCode());

            var arguments = new Dictionary<string, object>();
            var parameters = invocation.Method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                arguments[parameters[i].Name] = invocation.Arguments[i];
            }

            await SetRequest(new ServiceRequestContext(context, arguments, new RouteData(), serviceDescriptor));

            await _next(context);
        }

        #region Private Method

        private Task SetRequest(ServiceRequestContext serviceRequestContext)
        {
            return _requestMessageBuilder.BuildAsync(new RequestMessageBuilderContext(serviceRequestContext));
        }

        #endregion Private Method
    }
}