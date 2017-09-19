using Castle.DynamicProxy;

namespace Rabbit.Cloud.Facade.Features
{
    public interface IInvocationFeature
    {
        IInvocation Invocation { get; }
    }

    public class InvocationFeature : IInvocationFeature
    {
        public InvocationFeature(IInvocation invocation)
        {
            Invocation = invocation;
        }

        #region Implementation of IInvocationFeature

        public IInvocation Invocation { get; }

        #endregion Implementation of IInvocationFeature
    }
}