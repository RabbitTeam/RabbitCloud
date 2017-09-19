using Rabbit.Cloud.Facade.Abstractions;

namespace Rabbit.Cloud.Facade.Utilities.Extensions
{
    public static class ServiceRequestContextExtensions
    {
        public static object GetArgument(this ServiceRequestContext serviceRequestContext, string parameterName)
        {
            serviceRequestContext.Arguments.TryGetValue(parameterName, out var value);
            return value;
        }
    }
}