using System.ComponentModel.DataAnnotations;

namespace UnitConversion.Api.Models;

/// <summary>A request to convert a numeric value from one unit to another.</summary>
public sealed class ConversionRequest
{
    /// <summary>Maximum accepted length of a unit name; guards against oversized payloads.</summary>
    public const int MaxUnitNameLength = 50;

    /// <summary>The value to convert. Decimal and (where valid for the category) negative values are accepted.</summary>
    /// <example>100</example>
    [Required(ErrorMessage = "The 'value' field is required.")]
    public decimal? Value { get; init; }

    /// <summary>Unit of <see cref="Value"/>. Case-insensitive; symbols such as "m" or "ft" are accepted.</summary>
    /// <example>Meter</example>
    [Required(AllowEmptyStrings = false, ErrorMessage = "The 'fromUnit' field is required.")]
    [StringLength(MaxUnitNameLength, ErrorMessage = "The 'fromUnit' field must be at most {1} characters.")]
    public string FromUnit { get; init; } = string.Empty;

    /// <summary>Unit to convert to. Must be in the same category as <see cref="FromUnit"/>.</summary>
    /// <example>Foot</example>
    [Required(AllowEmptyStrings = false, ErrorMessage = "The 'toUnit' field is required.")]
    [StringLength(MaxUnitNameLength, ErrorMessage = "The 'toUnit' field must be at most {1} characters.")]
    public string ToUnit { get; init; } = string.Empty;
}
