# RestApiConfigurationProvider

RestApiConfigurationProvider is a .NET NuGet package that provides a simple and effective way to interact with Configuration REST API service. \
It offers a service SDK client (`IConfigurationRestApiClient`) interface and `RestApiConfigurationProvider`.

## Features

- Easy to set up client interface for Configuration REST API (`IConfigurationRestApiClient`).
- Seamless integration with .NET Core microservices for retrieving configuration in transparent way.

## Getting Started

### Usage

#### TL;DR

Package included working demo application where you can find fully configured example:
TODO: Add link to demo project Program.cs

### Configuration Example

Add the following configuration to your `appsettings.json`  file, adjusting the queue names and settings as necessary:

```json
{
    "ConfigurationHttpClientSettings": {
        "BaseUrl": "https://restapi.com/api",
        // Or define Environment Variable with name: `ConfigurationHttpClientSettings__ApiKey`
        "ApiKey": "abababab-abab-abab-abab-abababababab",
        "HttpClientTimeoutSeconds": "10"
    },
    "RestApiConfigurationProvider": {
        "CacheDurationMinutes": "60",
        "CacheKeyPrefix": "configuration_cache_prefix",
        "ConfigurationKeys": [
            "something-not-exist",
            "auth",
            "playback"
        ]
    }
}
```

#### In Program.cs

In your `Program.cs`, configure the services as follows:

```csharp
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


    var host = new WebHostBuilder()
        ...
        .ConfigureServices(services =>
        {
            // Inside the ConfigureServices method, add all the services from the appStartServiceCollection to the web host's services.
            // Optionally to reuse created instances in other part of applicateion
            foreach (var serviceDescriptor in appStartServiceCollection)
            {
                services.Add(serviceDescriptor);
            }
        })
        .ConfigureAppConfiguration((context, builder) =>
        {
            var restApiHttpClient = appStartServiceProvider.GetRequiredService<IRestApiHttpClient>();
            var configurationProviderSettings = preliminaryConfig
                .GetSection(RestApiConfigurationProviderSettings.SectionName)
                .Get<RestApiConfigurationProviderSettings>();

            builder.AddRestApiConfigurationSource(loggerFactory, distributedCache, restApiHttpClient, configurationProviderSettings);
        })
        ...
        .Build();
}
```

### Initial Setup

Begin by constructing a preliminary configuration using ConfigurationBuilder.
This loads the settings from:
- A JSON file named appsettings.json which contains general application settings.
- A JSON file named appsettings.package.json which may include package-specific settings.
- Environment variables which can override settings for different environments.
```csharp
var preliminaryConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.package.json")
    .AddEnvironmentVariables()
    .Build();
```

### Logger and Distributed Cache, Service Collection and Provider

Next, create a `LoggerFactory` and a `IDistributedCache` instance. These will be used for logging and caching purposes respectively.

Create a `ServiceCollection`, add the `loggerFactory`,
- register in DI the IConfigurationRestApiClient ,
- register in DI the IDistributedCache that will share configuration across multiple nodes(app instances)

as singletons to the service collection.

And build a service provider from the service collection which will be used to resolve services before application startup.

```csharp
var loggerFactory = GetFactory();
var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()), loggerFactory);

var appStartServiceCollection = new ServiceCollection()
    .AddSingleton(loggerFactory)
    .AddConfigurationRestApiClient(preliminaryConfig)
    .AddSingleton<IDistributedCache>(distributedCache);

var appStartServiceProvider = appStartServiceCollection.BuildServiceProvider();
```

### Configuration Provider Setup
- Obtain the required services like IRestApiHttpClient from the service provider.
- Retrieve configuration provider settings from the preliminary configuration
- Call `AddRestApiConfigurationSource` to build `ConfigurationSource` and create `RestApiConfigurationProvider` instance.
```csharp
.ConfigureAppConfiguration((context, builder) =>
{
    var restApiHttpClient = appStartServiceProvider.GetRequiredService<IRestApiHttpClient>();
    var configurationProviderSettings = preliminaryConfig
        .GetSection(RestApiConfigurationProviderSettings.SectionName)
        .Get<RestApiConfigurationProviderSettings>();

    builder.AddRestApiConfigurationSource(loggerFactory, distributedCache, restApiHttpClient, configurationProviderSettings);
})
```
