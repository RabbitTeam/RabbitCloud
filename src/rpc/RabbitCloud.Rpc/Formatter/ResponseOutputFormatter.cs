using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using System;
using System.IO;

namespace RabbitCloud.Rpc.Formatter
{
    public abstract class ResponseOutputFormatter : IOutputFormatter<IResponse>
    {
        #region Implementation of IOutputFormatter<in IResponse>

        public byte[] Format(IResponse instance)
        {
            using (var memoryStream = new MemoryStream())
            {
                var idData = BitConverter.GetBytes(instance.RequestId);
                memoryStream.Write(idData, 0, idData.Length);
                DoFormat(instance, memoryStream);
                return memoryStream.ToArray();
            }
        }

        #endregion Implementation of IOutputFormatter<in IResponse>

        protected abstract void DoFormat(IResponse response, MemoryStream memoryStream);
    }
}