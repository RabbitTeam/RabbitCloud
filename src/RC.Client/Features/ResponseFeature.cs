using System.IO;

namespace Rabbit.Cloud.Client.Features
{
    public class ResponseFeature : IResponseFeature
    {
        public ResponseFeature()
        {
            StatusCode = 200;
            Body = Stream.Null;
        }

        #region Implementation of IResponseFeature

        public int StatusCode { get; set; }
        public Stream Body { get; set; }
        public virtual bool HasStarted => false;

        #endregion Implementation of IResponseFeature
    }
}