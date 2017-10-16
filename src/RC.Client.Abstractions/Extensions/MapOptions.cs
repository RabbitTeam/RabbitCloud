namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public class MapOptions
    {
        public string PathMatch { get; set; }

        public RabbitRequestDelegate Branch { get; set; }
    }
}