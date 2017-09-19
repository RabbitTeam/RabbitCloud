using System.Threading.Tasks;

namespace RC.Abstractions
{
    public delegate Task RabbitRequestDelegate(RabbitContext rabbitContext);
}