using Microsoft.Extensions.Options;
using Rabbit.Go.Abstractions.Codec;
using System;
using System.Net.Http;

namespace Rabbit.Go.Core.Codec
{
    public class ForamtterDecoder : IDecoder
    {
        private readonly GoOptions _goOptions;

        public ForamtterDecoder(GoOptions goOptions)
        {
            _goOptions = goOptions;
        }

        public ForamtterDecoder(IOptions<GoOptions> goOptions)
        {
            _goOptions = goOptions.Value;
        }

        #region Implementation of IDecoder

        public object Decode(HttpResponseMessage response, Type type)
        {
            var mediaType = response.Content.Headers.ContentType.MediaType;

            return _goOptions.FormatterMappings.GetDecoder(mediaType)?.Decode(response, type);
        }

        #endregion Implementation of IDecoder
    }
}