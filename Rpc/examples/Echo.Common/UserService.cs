using Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Threading.Tasks;

namespace Echo.Common
{
    public class UserModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [RpcService]
    public interface IUserService
    {
        Task<string> GetUserName(int id);

        Task<bool> Exists(int id);

        Task<int> GetUserId(string userName);

        Task<DateTime> GetUserLastSignInTime(int id);

        Task<UserModel> GetUser(int id);

        Task<bool> Update(int id, UserModel model);
    }

    public class UserService : IUserService
    {
        #region Implementation of IUserService

        public Task<string> GetUserName(int id)
        {
            return Task.FromResult($"id:{id} is name rabbit.");
        }

        public Task<bool> Exists(int id)
        {
            return Task.FromResult(true);
        }

        public Task<int> GetUserId(string userName)
        {
            return Task.FromResult(1);
        }

        public Task<DateTime> GetUserLastSignInTime(int id)
        {
            return Task.FromResult(DateTime.Now);
        }

        public Task<UserModel> GetUser(int id)
        {
            return Task.FromResult(new UserModel
            {
                Name = "rabbit",
                Age = 18
            });
        }

        public Task<bool> Update(int id, UserModel model)
        {
            Console.WriteLine(model.Name);
            Console.WriteLine(model.Age);
            return Task.FromResult(true);
        }

        #endregion Implementation of IUserService
    }
}