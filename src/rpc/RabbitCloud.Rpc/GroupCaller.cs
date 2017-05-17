using RabbitCloud.Rpc.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc
{
    public abstract class GroupCaller : IGroupCaller
    {
        #region Implementation of ICaller

        public Task<IResponse> CallAsync(IRequest request)
        {
            var name = request.Key.Name;
            var caller = Callers.FirstOrDefault(i => i.Name == name);
            return caller.CallAsync(request);
        }

        #endregion Implementation of ICaller

        #region Implementation of IGroupCaller

        public abstract IEnumerable<INamedCaller> Callers { get; }

        #endregion Implementation of IGroupCaller
    }
}