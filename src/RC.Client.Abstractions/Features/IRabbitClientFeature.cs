namespace Rabbit.Cloud.Client.Abstractions.Features
{
    public interface IRabbitClientFeature
    {
        ServiceRequestOptions RequestOptions { get; set; }
    }
}