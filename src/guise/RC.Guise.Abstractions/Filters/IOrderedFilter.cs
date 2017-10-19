namespace Rabbit.Cloud.Guise.Abstractions.Filters
{
    public interface IOrderedFilter
    {
        int Order { get; }
    }
}