using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestApiConfigurationProvider.HttpClients;
using RestApiConfigurationProvider.HttpClients.DelegatingHeaders;
using Xunit;

namespace RestApiConfigurationProvider.Test.HttpClients;

public class ProgramExtensionsTests
{
    [Fact]
    public void AddConfigurationRestApiClient_RegistersServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConfigurationHttpClientSettings:BaseUrl", "http://example.com" }
            })
            .Build();

        // Act
        services.AddConfigurationRestApiClient(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<IRestApiHttpClient>());
        Assert.NotNull(serviceProvider.GetService<IRestApiAuthHttpClient>());
        Assert.NotNull(serviceProvider.GetService<AuthHeaderHandler>());
        Assert.NotNull(serviceProvider.GetService<LogResponseHandler>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddConfigurationRestApiClient_ThrowsException_WhenBaseUrlIsInvalid(string baseUrl)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConfigurationHttpClientSettings:BaseUrl", baseUrl },
                { "ConfigurationHttpClientSettings:HttpTimeoutSeconds", "10" }
            })
            .Build();


        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddConfigurationRestApiClient(configuration));
    }

    [Fact]
    public void AddConfigurationRestApiClient_ConfiguresHttpClientCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConfigurationHttpClientSettings:BaseUrl", "http://example.com" },
                { "ConfigurationHttpClientSettings:HttpTimeoutSeconds", "10" }
            })
            .Build();


        // Act
        services.AddConfigurationRestApiClient(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var restApiHttpClient = serviceProvider.GetRequiredService<IRestApiHttpClient>();

        // Assert
        Assert.NotNull(restApiHttpClient);
    }
}
