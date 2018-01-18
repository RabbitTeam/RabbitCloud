namespace Rabbit.Go.Abstractions.Filters
{
    public interface IOrderedFilter : IFilterMetadata
    {
        int Order { get; }
    }
}