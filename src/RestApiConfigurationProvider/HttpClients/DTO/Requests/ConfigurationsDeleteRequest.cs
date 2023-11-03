using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestApiConfigurationProvider.HttpClients.DTO.Requests;

/// <summary>
/// Represents a request to delete configurations.
/// </summary>
public class ConfigurationsDeleteRequest : IValidatableObject
{
    /// <summary>
    /// Gets or sets the specific version number of the configuration to delete.
    /// </summary>
    /// <value>
    /// An integer representing the version number.
    /// </value>
    public int? VersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the start of the version number range for deleting configurations.
    /// </summary>
    /// <value>
    /// An integer representing the start of the version number range.
    /// </value>
    public int? VersionNumberRangeStart { get; set; }

    /// <summary>
    /// Gets or sets the end of the version number range for deleting configurations.
    /// </summary>
    /// <value>
    /// An integer representing the end of the version number range.
    /// </value>
    public int? VersionNumberRangeEnd { get; set; }

    /// <summary>
    /// Validates the properties of this request object.
    /// </summary>
    /// <param name="validationContext">Describes the context in which the validation check is performed.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();

        if (!VersionNumber.HasValue && !VersionNumberRangeStart.HasValue && !VersionNumberRangeEnd.HasValue)
        {
            validationResults.Add(new ValidationResult(
                "Version number information is required.",
                new string[] { nameof(VersionNumber), nameof(VersionNumberRangeStart), nameof(VersionNumberRangeEnd) }));
        }

        if (VersionNumber.HasValue && (VersionNumberRangeStart.HasValue || VersionNumberRangeEnd.HasValue))
        {
            validationResults.Add(new ValidationResult(
                "Request may contain a version number or version number range.  Not both.",
                new string[] { nameof(VersionNumber), nameof(VersionNumberRangeStart), nameof(VersionNumberRangeEnd) }));
        }

        if ((VersionNumberRangeStart.HasValue && VersionNumberRangeEnd.HasValue && VersionNumberRangeStart.Value > VersionNumberRangeEnd.Value) ||
            (VersionNumberRangeStart.HasValue && !VersionNumberRangeEnd.HasValue) || (!VersionNumberRangeStart.HasValue && VersionNumberRangeEnd.HasValue))
        {
            validationResults.Add(new ValidationResult(
                "Version number range is invalid.",
                new string[] { nameof(VersionNumberRangeStart), nameof(VersionNumberRangeEnd) }));
        }

        return validationResults;
    }
}
