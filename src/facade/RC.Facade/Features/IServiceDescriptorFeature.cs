using Rabbit.Cloud.Facade.Abstractions;

namespace Rabbit.Cloud.Facade.Features
{
    public interface IServiceDescriptorFeature
    {
        ServiceDescriptor ServiceDescriptor { get; }
    }

    public class ServiceDescriptorFeature : IServiceDescriptorFeature
    {
        public ServiceDescriptorFeature(ServiceDescriptor serviceDescriptor)
        {
            ServiceDescriptor = serviceDescriptor;
        }

        #region Implementation of IServiceDescriptorFeature

        public ServiceDescriptor ServiceDescriptor { get; }

        #endregion Implementation of IServiceDescriptorFeature
    }
}