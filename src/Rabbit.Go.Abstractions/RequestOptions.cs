using System;

namespace Rabbit.Go
{
    public class RequestOptions
    {
        public static RequestOptions Default { get; } = new RequestOptions();

        public RequestOptions()
        {
            Timeout = TimeSpan.FromSeconds(60);
        }

        public TimeSpan Timeout { get; set; }
    }
}