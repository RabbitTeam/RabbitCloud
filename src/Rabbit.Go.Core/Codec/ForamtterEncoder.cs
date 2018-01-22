using Microsoft.Extensions.Options;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using System;

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

        public void Encode(object instance, Type type, RequestContext requestContext)
        {
            var headers = requestContext.Headers;
            headers.TryGetValue("Content-Type", out var value);
            var encoder = _goOptions.FormatterMappings.GetEncoder(value);

            encoder?.Encode(instance, type, requestContext);
        }

        #endregion Implementation of IEncoder
    }
}