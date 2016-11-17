using System.Threading.Tasks;

namespace RabbitCloud.Config.Abstractions
{
    public interface IApplicationBuilder
    {
        Task<ApplicationEntry> Build();
    }
}