using Rabbit.Go.Codec;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Core.Codec
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class EncoderAttribute : Attribute, IEncoder
    {
        #region Implementation of IEncoder

        public virtual Task EncodeAsync(object instance, Type type, RequestContext requestContext)
        {
            return Task.CompletedTask;
        }

        #endregion Implementation of IEncoder
    }
}