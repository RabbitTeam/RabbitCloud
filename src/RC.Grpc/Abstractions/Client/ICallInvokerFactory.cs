using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.Abstractions.Client
{
    public interface ICallInvokerFactory
    {
        Task<CallInvoker> GetCallInvokerAsync(string host, int port, TimeSpan timeout);
    }
}