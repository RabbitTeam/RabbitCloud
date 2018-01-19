namespace Rabbit.Go.Abstractions.Filters
{
    public interface IFilterContainer
    {
        IFilterMetadata FilterDefinition { get; set; }
    }
}