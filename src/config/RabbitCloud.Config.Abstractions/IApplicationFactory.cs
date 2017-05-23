using System.Threading.Tasks;

namespace RabbitCloud.Config.Abstractions
{
    public interface IApplicationFactory
    {
        Task<ApplicationModel> CreateApplicationAsync(ApplicationModelDescriptor descriptor);
    }
}