using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Go.Codec
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

        public async Task<object> DecodeAsync(HttpResponseMessage response, Type type)
        {
            var mediaType = response.Content.Headers.ContentType.MediaType;

            var decoder = _goOptions.FormatterMappings.GetDecoder(mediaType);

            if (decoder == null)
                return null;

            return await decoder.DecodeAsync(response, type);
        }

        #endregion Implementation of IDecoder
    }
}