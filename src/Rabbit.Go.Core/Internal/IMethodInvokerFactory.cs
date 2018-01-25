namespace Rabbit.Go.Core.Internal
{
    public interface IMethodInvokerFactory
    {
        IMethodInvoker CreateInvoker(MethodDescriptor methodDescriptor);
    }
}