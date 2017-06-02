using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using System;
using System.IO;

namespace RabbitCloud.Rpc.Formatter
{
    public abstract class RequestOutputFormatter : IOutputFormatter<IRequest>
    {
        #region Implementation of IOutputFormatter<in IRequest>

        public byte[] Format(IRequest instance)
        {
            using (var memoryStream = new MemoryStream())
            {
                var idData = BitConverter.GetBytes(instance.RequestId);
                memoryStream.Write(idData, 0, idData.Length);
                DoFormat(instance, memoryStream);
                return memoryStream.ToArray();
            }
        }

        #endregion Implementation of IOutputFormatter<in IRequest>

        protected abstract void DoFormat(IRequest request, MemoryStream memoryStream);
    }
}