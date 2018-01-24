using Rabbit.Go.Codec;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Go.Core.Codec
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class DecoderAttribute : Attribute, IDecoder
    {
        #region Implementation of IDecoder

        public virtual Task<object> DecodeAsync(HttpResponseMessage response, Type type)
        {
            return Task.FromResult<object>(null);
        }

        #endregion Implementation of IDecoder
    }
}