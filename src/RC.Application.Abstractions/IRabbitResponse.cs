namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitResponse
    {
        IRabbitContext RabbitContext { get; }
        object Response { get; set; }
    }
}