using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib.Proxies;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace App
{
    public class Functions
    {
        private const string TimeoutExpression = "00:00:30";
        private const string ScheduleExpression = "*/5 * * * * *";

        private readonly IApiProxy _apiProxy;

        public Functions(IApiProxy apiProxy)
        {
            _apiProxy = apiProxy;
        }

        [Singleton]
        [Timeout(TimeoutExpression)]
        public async Task RunAsync(
            [TimerTrigger(ScheduleExpression, RunOnStartup = true)]
            TimerInfo timer,
            CancellationToken cancellationToken,
            ILogger logger)
        {
            var data = await _apiProxy.GetRandomDataAsync();
            var json = GetFormattedJson(data);
            var now = DateTime.Now;
            logger.LogInformation("Found result at {now}: {json}", now, json);
        }

        private static string GetFormattedJson(string json)
        {
            var stringBuilder = new StringBuilder();

            foreach (var (key, value) in JObject.Parse(json))
            {
                stringBuilder.AppendLine($"{key}: {value}");
            }

            return stringBuilder.ToString();
        }
    }
}
