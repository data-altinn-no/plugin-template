using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.DATASOURCENAME.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Caching.Distributed;
using Polly.Extensions.Http;
using Polly.Registry;

namespace Altinn.Dan.Plugin.DATASOURCENAME
{
    class Program
    {
        private static ApplicationSettings ApplicationSettings { get; set; }

        private static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    // See https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service#using-application-insights-sdk-for-worker-services
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.AddHttpClient();

                    services.AddOptions<ApplicationSettings>()
                        .Configure<IConfiguration>((settings, configuration) => configuration.Bind(settings));
                    ApplicationSettings = services.BuildServiceProvider().GetRequiredService<IOptions<ApplicationSettings>>().Value;

                    services.AddStackExchangeRedisCache(option => { option.Configuration = ApplicationSettings.RedisConnectionString; });

                    var distributedCache = services.BuildServiceProvider().GetRequiredService<IDistributedCache>();
                    var registry = new PolicyRegistry()
                    {
                        { "defaultCircuitBreaker", HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(4, ApplicationSettings.BreakerRetryWaitTime) },
                        { "CachePolicy", Policy.CacheAsync(distributedCache.AsAsyncCacheProvider<string>(), TimeSpan.FromHours(12)) }
                    };
                    services.AddPolicyRegistry(registry);

                    // Client configured with circuit breaker policies
                    services.AddHttpClient("SafeHttpClient", client => { client.Timeout = new TimeSpan(0, 0, 30); })
                        .AddPolicyHandlerFromRegistry("defaultCircuitBreaker");

                    // Client configured without circuit breaker policies. shorter timeout
                    services.AddHttpClient("CachedHttpClient", client => { client.Timeout = new TimeSpan(0, 0, 5); });

                    // Client configured with enterprise certificate authentication
                    services.AddHttpClient("ECHttpClient", client =>
                        {
                            client.DefaultRequestHeaders.Add("Accept", "application/json");
                        })
                        .AddPolicyHandlerFromRegistry("defaultCircuitBreaker")
                        .ConfigurePrimaryHttpMessageHandler(() =>
                        {
                            var handler = new HttpClientHandler();
                            handler.ClientCertificates.Add(ApplicationSettings.Certificate);

                            return handler;
                        });

                    //Newtonsoft.Json
                    JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                    {
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter>() { new StringEnumConverter() }
                    };

                    //System.Text.Json
                    // services.Configure<JsonSerializerOptions>(options =>
                    // {
                    //     options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    //     options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    //     options.Converters.Add(new JsonStringEnumConverter());
                    // });
                })
                .Build();

            return host.RunAsync();
        }
    }
}
