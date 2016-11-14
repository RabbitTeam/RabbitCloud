using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Utils;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public class RefererInvocationHandler : IInvocationHandler
    {
        private readonly IReferer _referer;

        public RefererInvocationHandler(IReferer referer)
        {
            _referer = referer;
        }

        #region Implementation of IInvocationHandler

        public async Task<object> Invoke(object proxy, MethodInfo method, object[] args)
        {
            var response = await _referer.Call(new DefaultRequest
            {
                Arguments = args,
                InterfaceName = "",
                MethodName = method.Name,
                ParamtersType = method.GetParameters().Select(i => i.ParameterType.FullName).ToArray(),
                RequestId = MessageIdGenerator.GeneratorId()
            });

            return response.Result;
        }

        #endregion Implementation of IInvocationHandler
    }
}