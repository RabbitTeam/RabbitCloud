using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Discovery.Abstractions
{
    public interface IServiceInstance
    {
        string ServiceId { get; }
        string Host { get; }
        int Port { get; }
        bool IsSecure { get; }
        Uri Uri { get; }
        IDictionary<string, string> Metadata { get; }
    }
}