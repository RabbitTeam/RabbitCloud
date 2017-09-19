using Rabbit.Cloud.Facade.Abstractions;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageBuilderContext
    {
        public RequestMessageBuilderContext(ServiceRequestContext serviceRequestContext)
        {
            ServiceRequestContext = serviceRequestContext;
        }

        public ServiceRequestContext ServiceRequestContext { get; }
    }
}