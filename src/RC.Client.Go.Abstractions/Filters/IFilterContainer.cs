namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public interface IFilterContainer
    {
        IFilterMetadata FilterDefinition { get; set; }
    }
}