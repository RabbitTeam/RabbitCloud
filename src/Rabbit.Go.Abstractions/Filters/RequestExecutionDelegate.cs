using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions.Filters
{
    public delegate Task<RequestExecutedContext> RequestExecutionDelegate();
}