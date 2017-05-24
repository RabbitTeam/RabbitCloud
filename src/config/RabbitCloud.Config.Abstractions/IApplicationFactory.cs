using System.Threading.Tasks;

namespace RabbitCloud.Config.Abstractions
{
    public interface IApplicationFactory
    {
        Task<IApplicationModel> CreateApplicationAsync(ApplicationModelDescriptor descriptor);
    }
}