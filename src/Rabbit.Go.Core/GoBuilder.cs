/*using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using System;
using System.Collections.Generic;
using Rabbit.Go.Core.Formatters;

namespace Rabbit.Go.Core
{
    public class GoBuilder
    {
        private readonly IList<IGoInterceptor> _goInterceptors = new List<IGoInterceptor>();
        private IGoClient _goClient;
        private IEncoder _encoder;
        private IDecoder _decoder;

        public GoBuilder Encoder(IEncoder encoder)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            return this;
        }

        public GoBuilder Decoder(IDecoder decoder)
        {
            _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
            return this;
        }

        public GoBuilder GoClient(IGoClient goClient)
        {
            _goClient = goClient ?? throw new ArgumentNullException(nameof(goClient));
            return this;
        }

        public GoBuilder RequestInterceptor(params IGoInterceptor[] goInterceptors)
        {
            if (goInterceptors == null)
                throw new ArgumentNullException(nameof(goInterceptors));

            foreach (var goInterceptor in goInterceptors)
            {
                if (goInterceptor == null)
                    continue;

                _goInterceptors.Add(goInterceptor);
            }

            return this;
        }

        public Go Build()
        {
            return new DefaultGo(_goInterceptors,keyValueFormatterFactory:null,methodDescriptorCollectionProvider:null, _goClient, _decoder);
        }
    }
}*/