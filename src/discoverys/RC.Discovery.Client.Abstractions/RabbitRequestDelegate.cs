using System.Threading.Tasks;

namespace RC.Discovery.Client.Abstractions
{
    public delegate Task RabbitRequestDelegate(RabbitContext rabbitContext);
}