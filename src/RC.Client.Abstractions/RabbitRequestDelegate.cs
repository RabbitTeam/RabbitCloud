using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public delegate Task RabbitRequestDelegate(RabbitContext rabbitContext);
}