using System;
using System.Threading.Tasks;
using Lib.Configuration;
using Lib.Proxies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            hostBuilder
                .ConfigureWebJobs(builder =>
                {
                    builder.AddAzureStorageCoreServices();
                    builder.AddAzureStorage();
                    builder.AddTimers();
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentVariables();
                    var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "DEV";
                    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    builder.AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddNonGenericLogger();
                    builder.AddConsole(options =>
                    {
                        options.DisableColors = false;
                        options.TimestampFormat = "[HH:mm:ss:fff] ";
                    });
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    var instrumentationKey = context.Configuration["Settings:ApplicationInsights:InstrumentationKey"];
                    builder.AddApplicationInsightsWebJobs(options => options.InstrumentationKey = instrumentationKey);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<Settings>(context.Configuration.GetSection(nameof(Settings)));
                    services.AddHttpClient<IApiProxy, ApiProxy>()
                        .ConfigureHttpClient((provider, client) =>
                        {
                            var settings = provider.GetService<IOptions<Settings>>().Value;
                            var baseAddress = settings.ExternalApi.BaseAddress;
                            client.BaseAddress = new Uri(baseAddress);
                            client.DefaultRequestHeaders.Add("Accept", "application/json");
                        });
                });

            var host = hostBuilder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }

        private static void AddNonGenericLogger(this ILoggingBuilder loggingBuilder)
        {
            var services = loggingBuilder.Services;
            services.AddSingleton(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger("ContinuousWebJobDemo");
            });
        }
    }
}
