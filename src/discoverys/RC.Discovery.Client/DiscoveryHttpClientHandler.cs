using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Client
{
    public class DiscoveryHttpClientHandler : HttpClientHandler
    {
        private readonly IDiscoveryClient _discoveryClient;
        private readonly ILogger<DiscoveryHttpClientHandler> _logger;
        private static readonly Random Random = new Random();

        public DiscoveryHttpClientHandler(IDiscoveryClient discoveryClient, ILogger<DiscoveryHttpClientHandler> logger)
        {
            _discoveryClient = discoveryClient;
            _logger = logger;
        }

        #region Overrides of HttpMessageHandler

        #region Overrides of HttpClientHandler

        /// <inheritdoc />
        /// <summary>Creates an instance of  <see cref="T:System.Net.Http.HttpResponseMessage"></see> based on the information provided in the <see cref="T:System.Net.Http.HttpRequestMessage"></see> as an operation that will not block.</summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request">request</paramref> was null.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            if (!current.IsDefaultPort)
                return await base.SendAsync(request, cancellationToken);

            try
            {
                request.RequestUri = LookupService(current);
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                request.RequestUri = current;
            }
        }

        #endregion Overrides of HttpClientHandler

        private Uri LookupService(Uri current)
        {
            _logger?.LogDebug("LookupService({0})", current.ToString());

            var instances = _discoveryClient.GetInstances(current.Host);

            if (instances != null && instances.Any())
            {
                var index = Random.Next(instances.Count);
                current = new Uri(instances.ElementAt(index).Uri, current.PathAndQuery);
            }
            _logger?.LogDebug("LookupService() returning {0} ", current.ToString());
            return current;
        }

        #endregion Overrides of HttpMessageHandler
    }
}