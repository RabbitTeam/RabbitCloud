using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public delegate Task<RequestExecutedContext> RequestExecutionDelegate();
}