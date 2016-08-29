using System;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IExporter : IDisposable
    {
        IInvoker Invoker { get; }
    }
}