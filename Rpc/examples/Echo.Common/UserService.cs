using Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace Echo.Common
{
    [RpcService]
    public interface IUserService
    {
        Task<string> GetUserName(int id);
    }

    public class UserService : IUserService
    {
        #region Implementation of IUserService

        public Task<string> GetUserName(int id)
        {
            return Task.FromResult($"id:{id} is name rabbit.");
        }

        #endregion Implementation of IUserService
    }
}