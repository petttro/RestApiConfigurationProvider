using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RestApiConfigurationProvider.ConfigurationProviders;
using RestApiConfigurationProvider.Extensions;
using RestApiConfigurationProvider.HttpClients;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace RestApiConfigurationProvider.Demo;

// https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
// https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-2/
public class Program
{
    public static Task Main(string[] args)
    {
        var preliminaryConfig = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.package.json")
            .AddEnvironmentVariables()
            .Build();

        var loggerFactory = GetFactory();
        var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()), loggerFactory);

        var appStartServiceCollection = new ServiceCollection()
            .AddSingleton(loggerFactory)
            .AddConfigurationRestApiClient(preliminaryConfig)
            .AddSingleton<IDistributedCache>(distributedCache);

#pragma warning disable ASP0000
        var appStartServiceProvider = appStartServiceCollection.BuildServiceProvider();
#pragma warning restore ASP0000

        var host = new WebHostBuilder()
            .UseKestrel(options => { options.Listen(IPAddress.Any, port: 5000); })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureServices(services =>
            {
                foreach (var serviceDescriptor in appStartServiceCollection)
                {
                    services.Add(serviceDescriptor);
                }
            })
            .ConfigureAppConfiguration((context, builder) =>
            {
                var configurationRestApiClient = appStartServiceProvider.GetRequiredService<IConfigurationRestApiClient>();
                var configurationProviderSettings = preliminaryConfig.GetSection(RestApiConfigurationProviderSettings.SectionName)
                    .Get<RestApiConfigurationProviderSettings>();

                builder.AddRestApiConfigurationSource(loggerFactory, distributedCache, configurationRestApiClient, configurationProviderSettings);
            })
            .UseStartup<Startup>()
            .Build();

        host.Run();
        return Task.CompletedTask;
    }

    private static ILoggerFactory GetFactory()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddConsole();
        });

        return loggerFactory;
    }
}
