namespace Rabbit.Cloud.Client.Breaker.Features
{
    public interface IBreakerFeature
    {
        string Strategy { get; set; }
    }

    public class BreakerFeature : IBreakerFeature
    {
        #region Implementation of IBreakerFeature

        public string Strategy { get; set; }

        #endregion Implementation of IBreakerFeature
    }
}