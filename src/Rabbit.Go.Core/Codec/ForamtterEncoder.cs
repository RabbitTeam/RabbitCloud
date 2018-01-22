using Microsoft.Extensions.Options;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Core.Codec
{
    public class ForamtterEncoder : IEncoder
    {
        private readonly GoOptions _goOptions;

        public ForamtterEncoder(GoOptions goOptions)
        {
            _goOptions = goOptions;
        }

        public ForamtterEncoder(IOptions<GoOptions> goOptions)
        {
            _goOptions = goOptions.Value;
        }

        #region Implementation of IEncoder

        public async Task EncodeAsync(object instance, Type type, RequestContext requestContext)
        {
            var headers = requestContext.Headers;
            headers.TryGetValue("Content-Type", out var value);
            var encoder = _goOptions.FormatterMappings.GetEncoder(value);

            if (encoder != null)
                await encoder.EncodeAsync(instance, type, requestContext);
        }

        #endregion Implementation of IEncoder
    }
}