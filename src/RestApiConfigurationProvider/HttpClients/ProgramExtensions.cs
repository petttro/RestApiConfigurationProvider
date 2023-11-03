using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;
using RestApiConfigurationProvider.HttpClients.Config;
using RestApiConfigurationProvider.HttpClients.DelegatingHeaders;

namespace RestApiConfigurationProvider.HttpClients;

/// <summary>
/// Provides extension methods to register and configure ConfigurationRestApiClient services.
/// </summary>
public static class ProgramExtensions
{
    /// <summary>
    /// Adds and configures ConfigurationRestApiClient services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configuration">The application's configuration.</param>
    /// <returns>The updated IServiceCollection with RestApiHttpClient services added.</returns>
    /// <exception cref="ArgumentException">Thrown when the BaseUrl in the configuration is null or empty.</exception>
    public static IServiceCollection AddConfigurationRestApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        var restApiHttpClientSettings = configuration.GetSection(ConfigurationHttpClientSettings.SectionName).Get<ConfigurationHttpClientSettings>();
        services.AddConfigurationRestApiClient(restApiHttpClientSettings);

        return services;
    }

    /// <summary>
    /// Adds and configures REST API Http client services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configurationHttpClientSettings">The settings required for ConfigurationRestApiHttpClient.</param>
    /// <returns>The updated IServiceCollection with RestApiHttpClient services added.</returns>
    /// <exception cref="ArgumentException">Thrown when the BaseUrl in the configuration is null or empty.</exception>
    public static IServiceCollection AddConfigurationRestApiClient(this IServiceCollection services, ConfigurationHttpClientSettings configurationHttpClientSettings)
    {
        if (string.IsNullOrEmpty(configurationHttpClientSettings?.BaseUrl))
        {
            throw new ArgumentException($"{nameof(ConfigurationHttpClientSettings.BaseUrl)} setting must not be empty");
        }

        services.AddSingleton(configurationHttpClientSettings);

        services.TryAddTransient<AuthHeaderHandler>();
        services.TryAddTransient<LogResponseHandler>();

        services.AddRefitClient<IRestApiHttpClient>()
            .ConfigureHttpClient(httpClient => ConfigureHttpClient(httpClient, configurationHttpClientSettings))
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddHttpMessageHandler<LogResponseHandler>();

        services.AddRefitClient<IRestApiAuthHttpClient>()
            .ConfigureHttpClient(httpClient => ConfigureHttpClient(httpClient, configurationHttpClientSettings))
            .AddHttpMessageHandler<LogResponseHandler>();

        return services;
    }

    private static void ConfigureHttpClient(HttpClient httpClient, ConfigurationHttpClientSettings configurationHttpClientSettings)
    {
        httpClient.BaseAddress = new Uri(configurationHttpClientSettings.BaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(configurationHttpClientSettings.HttpTimeoutSeconds);
    }
}
