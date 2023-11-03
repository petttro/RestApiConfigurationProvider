using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestApiConfigurationProvider.Extensions;

namespace RestApiConfigurationProvider.HostedServices;

/// <summary>
/// Hosted service for reloading configuration periodically.
/// </summary>
public sealed class ConfigurationReloadHostedService : IHostedService, IDisposable
{
    /// <summary>
    /// Interval in seconds to reload the configuration.
    /// </summary>
    private const int ConfigurationReloadIntervalSeconds = 5;

    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationReloadHostedService> _logger;
    private PeriodicTimer _timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationReloadHostedService"/> class.
    /// </summary>
    /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
    /// <param name="logger">An instance of <see cref="ILogger"/>.</param>
    public ConfigurationReloadHostedService(IConfiguration configuration, ILogger<ConfigurationReloadHostedService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Starts the configuration reloading service.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the start operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(ConfigurationReloadIntervalSeconds));

        // Start task in background
        _ = ExecutePeriodicTask(cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the configuration reloading service.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the stop operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes of the resources used by the configuration reloading service.
    /// </summary>
    public void Dispose()
    {
        _timer.Dispose();
    }

    private async Task ExecutePeriodicTask(CancellationToken cancellationToken)
    {
        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {
            await LoadConfigurationAsync();
        }
    }

    private async Task LoadConfigurationAsync()
    {
        try
        {
            await _configuration.LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload local configuration");
        }
    }
}
