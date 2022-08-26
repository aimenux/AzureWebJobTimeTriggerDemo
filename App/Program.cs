using System;
using System.Threading.Tasks;
using App.Extensions;
using Lib.Configuration;
using Lib.Proxies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace App
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebJobs(builder =>
                {
                    builder.AddAzureStorageCoreServices();
                    builder.AddTimers();
                })
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddJsonFile();
                    config.AddUserSecrets();
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    loggingBuilder.AddLogging(hostingContext.Configuration);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.Configure<Settings>(hostingContext.Configuration.GetSection("Settings"));
                    services.AddHttpClient<IApiProxy, ApiProxy>()
                        .ConfigureHttpClient((provider, client) =>
                        {
                            var options = provider.GetRequiredService<IOptions<Settings>>();
                            var baseAddress = options.Value.ExternalApi.BaseAddress;
                            client.BaseAddress = new Uri(baseAddress);
                            client.DefaultRequestHeaders.Add("Accept", "application/json");
                        });
                });
    }
}
