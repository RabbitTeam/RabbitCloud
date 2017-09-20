namespace Rabbit.Cloud.Cluster.HighAvailability
{
    public enum HighAvailabilityStrategy
    {
        Failover,
        Failfast
    }

    public class HighAvailabilityOptions
    {
        public HighAvailabilityOptions()
        {
            Strategy = HighAvailabilityStrategy.Failover;
            RetryCount = 5;
        }

        public HighAvailabilityStrategy Strategy { get; set; }
        public uint RetryCount { get; set; }
    }
}