using Polly;

namespace Rabbit.Cloud.Client.Breaker.Features
{
    public interface IBreakerFeature
    {
        Policy Policy { get; set; }
    }

    public class BreakerFeature : IBreakerFeature
    {
        public Policy Policy { get; set; }
    }
}