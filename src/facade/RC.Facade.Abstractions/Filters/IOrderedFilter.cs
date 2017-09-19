namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public interface IOrderedFilter
    {
        int Order { get; }
    }
}