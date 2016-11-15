using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Hosting.Server.Features
{
    public interface IServerAddressesFeature
    {
        ICollection<string> Addresses { get; }
    }

    public class ServerAddressesFeature : IServerAddressesFeature
    {
        #region Implementation of IIServerAddressesFeature

        public ICollection<string> Addresses { get; } = new List<string>();

        #endregion Implementation of IIServerAddressesFeature
    }
}