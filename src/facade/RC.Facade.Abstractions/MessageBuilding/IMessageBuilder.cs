using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public interface IMessageBuilder
    {
        Task BuildAsync(MessageBuilderContext context);
    }
}