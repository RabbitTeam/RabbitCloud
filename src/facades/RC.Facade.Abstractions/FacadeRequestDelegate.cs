using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public delegate Task FacadeRequestDelegate(FacadeContext facadeContext);
}