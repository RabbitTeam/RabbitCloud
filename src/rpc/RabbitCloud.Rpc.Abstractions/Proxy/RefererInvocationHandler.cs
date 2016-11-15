using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Utils;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public class RefererInvocationHandler : IInvocationHandler
    {
        private readonly ICaller _caller;

        public RefererInvocationHandler(ICaller caller)
        {
            _caller = caller;
        }

        #region Implementation of IInvocationHandler

        public async Task<object> Invoke(object proxy, MethodInfo method, object[] args)
        {
            var response = await _caller.Call(new DefaultRequest
            {
                Arguments = args,
                InterfaceName = "",
                MethodName = method.Name,
                ParamtersType = method.GetParameters().Select(i => i.ParameterType.FullName).ToArray(),
                RequestId = MessageIdGenerator.GeneratorId()
            });

            if (response.Exception != null)
                throw response.Exception;

            return response.Result;
        }

        #endregion Implementation of IInvocationHandler
    }
}