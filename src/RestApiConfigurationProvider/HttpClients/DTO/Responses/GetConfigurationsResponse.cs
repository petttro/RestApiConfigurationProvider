using System.Collections.Generic;
using System.Linq;

namespace RestApiConfigurationProvider.HttpClients.DTO.Responses;

/// <summary>
/// Represents a response containing a list of configurations.
/// Inherits from a list of ConfigurationData.
/// </summary>
public class GetConfigurationsResponse : List<GetConfigurationsResponse.ConfigurationData>
{
    /// <summary>
    /// Initializes a new instance of the GetConfigurationsResponse class.
    /// </summary>
    /// <param name="configurations">A dictionary containing configuration names as keys and their versions as values.</param>
    public GetConfigurationsResponse(IDictionary<string, int> configurations)
    {
        AddRange(configurations.Select(x => new ConfigurationData(x.Key, x.Value)));
    }

    /// <summary>
    /// Represents the data of a single configuration.
    /// </summary>
    public class ConfigurationData
    {
        /// <summary>
        /// Initializes a new instance of the ConfigurationData class.
        /// </summary>
        /// <param name="name">The name of the configuration.</param>
        /// <param name="version">The version of the configuration.</param>
        public ConfigurationData(string name, int version)
        {
            ConfigurationName = name;
            Version = version;
        }

        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the version of the configuration.
        /// </summary>
        public int Version { get; set; }
    }
}
