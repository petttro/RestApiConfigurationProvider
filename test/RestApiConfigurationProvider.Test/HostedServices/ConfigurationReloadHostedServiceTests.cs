using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestApiConfigurationProvider.ConfigurationProviders;
using RestApiConfigurationProvider.HostedServices;
using Xunit;

namespace RestApiConfigurationProvider.Test.HostedServices;

public class ConfigurationReloadHostedServiceTests : MockStrictBehaviorTest
{
    private readonly ConfigurationReloadHostedService _service;
    private readonly Mock<IConfigurationRoot> _configurationMock;
    private readonly Mock<IRestApiConfigurationProvider> _configurationProviderMock;

    public ConfigurationReloadHostedServiceTests()
    {
        _configurationMock = _mockRepository.Create<IConfigurationRoot>();
        _configurationProviderMock = _mockRepository.Create<IRestApiConfigurationProvider>();
        _service = new ConfigurationReloadHostedService(_configurationMock.Object, new NullLogger<ConfigurationReloadHostedService>());
    }

    [Fact]
    public async Task StartAsync_ShouldStartTimer()
    {
        // Arrange
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        _configurationMock
            .SetupGet(p => p.Providers)
            .Returns(new List<IConfigurationProvider>() { _configurationProviderMock.Object });

        _configurationProviderMock
            .Setup(p => p.LoadAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(7)); // Give some time for the timer to tick.
        await _service.StopAsync(cts.Token);

        // Assert
        _configurationProviderMock.Verify(config => config.LoadAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoadConfigurationAsync_ShouldHandleException()
    {
        // Arrange
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        _configurationMock
            .SetupGet(p => p.Providers)
            .Returns(new List<IConfigurationProvider>() { _configurationProviderMock.Object });

        _configurationProviderMock
            .Setup(p => p.LoadAsync())
            .ThrowsAsync(new Exception("Test Exception"));

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(7)); // Give some time for the timer to tick.
        await _service.StopAsync(cts.Token);
    }
}
