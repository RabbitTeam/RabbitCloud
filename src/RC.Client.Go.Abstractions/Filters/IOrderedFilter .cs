namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public interface IOrderedFilter : IFilterMetadata
    {
        int Order { get; }
    }
}