using System.Threading.Tasks;

namespace Rabbit.Cloud.Application.Abstractions
{
    public delegate Task RabbitRequestDelegate(IRabbitContext rabbitContext);
}