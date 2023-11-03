using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
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
            .AddJsonFile("appsettings.json")
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
                var restApiHttpClient = appStartServiceProvider.GetRequiredService<IRestApiHttpClient>();
                var configurationProviderSettings = preliminaryConfig.GetSection(RestApiConfigurationProviderSettings.SectionName)
                    .Get<RestApiConfigurationProviderSettings>();

                builder.AddRestApiConfigurationSource(loggerFactory, distributedCache, restApiHttpClient, configurationProviderSettings);
            })
            .UseStartup<Startup>()
            .ConfigureLogging((context, builder) =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                LogManager.Configuration = new NLogLoggingConfiguration(context.Configuration.GetSection("NLog"));
            })
            .UseNLog()
            .Build();

        host.Run();
        return Task.CompletedTask;
    }

    private static ILoggerFactory GetFactory()
    {
        var factory = new LoggerFactory();
        factory.AddNLog();
        return factory;
    }
}
