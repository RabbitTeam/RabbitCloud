namespace Rabbit.Cloud.Application.Features
{
    public interface IQueryFeature
    {
        IQueryCollection Query { get; set; }
    }
}