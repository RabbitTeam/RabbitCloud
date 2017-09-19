using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Internal
{
    public interface IRequestMessageBuilder
    {
        Task BuildAsync(RequestMessageBuilderContext context);
    }
}