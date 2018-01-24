using System.Threading.Tasks;

namespace Rabbit.Go.Interceptors
{
    public delegate Task<RequestExecutedContext> RequestExecutionDelegate();
}