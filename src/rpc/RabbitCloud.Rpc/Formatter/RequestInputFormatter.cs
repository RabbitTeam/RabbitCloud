using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using System;
using System.Linq;

namespace RabbitCloud.Rpc.Formatter
{
    public abstract class RequestInputFormatter : IInputFormatter<IRequest>
    {
        #region Implementation of IInputFormatter<out IRequest>

        public IRequest Format(byte[] data)
        {
            var request = new Request
            {
                RequestId = BitConverter.ToInt64(data.Take(8).ToArray(), 0)
            };
            DoFormat(data.Skip(8).ToArray(), request);

            return request;
        }

        #endregion Implementation of IInputFormatter<out IRequest>

        protected abstract void DoFormat(byte[] data, Request request);
    }
}