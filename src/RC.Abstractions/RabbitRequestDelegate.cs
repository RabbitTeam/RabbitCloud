using System.Threading.Tasks;

namespace Rabbit.Cloud.Abstractions
{
    public delegate Task RabbitRequestDelegate(RabbitContext rabbitContext);
}