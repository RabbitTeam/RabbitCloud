using Rabbit.Go.Abstractions;
using Rabbit.Go.ApplicationModels;

namespace Rabbit.Go.Features
{
    public interface IGoFeature
    {
        RequestContext RequestContext { get; set; }
        RequestModel RequestModel { get; set; }
    }

    public class GoFeature : IGoFeature
    {
        #region Implementation of IGoFeature

        public RequestContext RequestContext { get; set; }
        public RequestModel RequestModel { get; set; }

        #endregion Implementation of IGoFeature
    }
}