using System.Threading.Tasks;

namespace Rabbit.Cloud.Guise.Abstractions.Building
{
    public interface IRequestBuilder
    {
        Task BuildAsync(BuildingContext buildingContext);
    }
}