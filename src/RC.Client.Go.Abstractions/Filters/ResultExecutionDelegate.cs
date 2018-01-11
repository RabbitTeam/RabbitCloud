using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public delegate Task<ResultExecutedContext> ResultExecutionDelegate();
}