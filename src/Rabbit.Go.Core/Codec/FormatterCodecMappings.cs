using Microsoft.Extensions.Primitives;
using Rabbit.Go.Abstractions.Codec;
using System;
using System.Collections.Generic;

namespace Rabbit.Go.Core.Codec
{
    public class FormatterCodecMappings
    {
        private readonly IDictionary<string, IEncoder> _encoders = new Dictionary<string, IEncoder>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, IDecoder> _decoders = new Dictionary<string, IDecoder>(StringComparer.OrdinalIgnoreCase);

        public FormatterCodecMappings SetEncoder(StringValues formats, IEncoder encoder)
        {
            foreach (var format in formats.ToArray())
                _encoders[format] = encoder;

            return this;
        }

        public FormatterCodecMappings SetDecoder(StringValues formats, IDecoder decoder)
        {
            foreach (var format in formats.ToArray())
                _decoders[format] = decoder;

            return this;
        }

        public FormatterCodecMappings Set(StringValues formats, IEncoder encoder, IDecoder decoder)
        {
            return
                SetEncoder(formats, encoder)
                    .SetDecoder(formats, decoder);
        }

        public IEncoder GetEncoder(string format)
        {
            return _encoders.TryGetValue(format, out var encoder) ? encoder : null;
        }

        public IDecoder GetDecoder(string format)
        {
            return _decoders.TryGetValue(format, out var decoder) ? decoder : null;
        }
    }
}