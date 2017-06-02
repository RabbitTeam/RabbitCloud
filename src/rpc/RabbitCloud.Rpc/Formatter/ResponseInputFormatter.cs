using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using System;
using System.Linq;

namespace RabbitCloud.Rpc.Formatter
{
    public abstract class ResponseInputFormatter : IInputFormatter<IResponse>
    {
        #region Implementation of IInputFormatter<out IResponse>

        public IResponse Format(byte[] data)
        {
            var response = new Response
            {
                RequestId = BitConverter.ToInt64(data.Take(8).ToArray(), 0)
            };
            DoFormat(data.Skip(8).ToArray(), response);

            return response;
        }

        #endregion Implementation of IInputFormatter<out IResponse>

        protected abstract void DoFormat(byte[] data, Response response);
    }
}