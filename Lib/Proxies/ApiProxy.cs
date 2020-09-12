using System;
using System.Net.Http;
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
            _client = client;
            _logger = logger;
            _queryString = options.Value.ExternalApi.QueryString;
            _logger.LogInformation("call to ctor ApiProxy at {now}", DateTime.Now);
        }

        public async Task<string> GetRandomDataAsync()
        {
            try
            {
                return await _client.GetStringAsync(_queryString);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("An error has occurred: {ex}", ex);
            }
            
            return null;
        }
    }
}
