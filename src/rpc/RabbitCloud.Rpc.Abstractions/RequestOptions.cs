using System;

namespace RabbitCloud.Rpc.Abstractions
{
    public struct RequestOptions
    {
        /// <summary>
        /// 请求超时时间，默认为10秒。
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// 是否抛出详细的异常，默认为true。
        /// </summary>
        public bool ThrowException { get; set; }

        /// <summary>
        /// 重试次数。
        /// </summary>
        public int Retries { get; set; }
    }

    public static partial class RequestExtensions
    {
        public static IRequest SetRequestOptions(this IRequest request, RequestOptions options)
        {
            request.SetAttachment(nameof(options.Timeout), ((int)options.Timeout.TotalSeconds).ToString());
            request.SetAttachment(nameof(options.ThrowException), options.ThrowException ? "true" : "false");
            request.SetAttachment(nameof(options.Retries), options.Retries.ToString());

            return request;
        }

        public static RequestOptions GetRequestOptions(this IRequest request)
        {
            var timeout = request.GetAttachment("timeout", value => !int.TryParse(value, out int number) ? (false, TimeSpan.Zero) : (true, TimeSpan.FromSeconds(number)), TimeSpan.FromSeconds(10));
            var throwDetailedException = string.Equals(bool.TrueString, request.GetAttachment("throwDetailedException"),
                StringComparison.OrdinalIgnoreCase);
            var retries = request.GetAttachment("retries", value =>
            {
                var success = int.TryParse(value, out int number);
                return (success, number);
            }, 0);

            var options = new RequestOptions
            {
                Timeout = timeout,
                ThrowException = throwDetailedException,
                Retries = retries
            };

            return options;
        }
    }
}