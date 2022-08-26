using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lib.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lib.Proxies
{
    public class ApiProxy : IApiProxy
    {
        private readonly string _queryString;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public ApiProxy(HttpClient client, IOptions<Settings> options, ILogger logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryString = options.Value.ExternalApi.QueryString;
            _logger.LogInformation("Call to ApiProxy ctor at {now}", DateTime.Now);
        }

        public async Task<string> GetRandomDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _client.GetStringAsync(_queryString, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error has occurred: {error}", ex.Message);
            }

            return null;
        }
    }
}
