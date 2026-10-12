using UnitConversion.Api.Units;

namespace UnitConversion.Api.Models;

/// <summary>The result of a successful conversion.</summary>
/// <param name="OriginalValue">The value that was submitted.</param>
/// <param name="FromUnit">Canonical name of the source unit.</param>
/// <param name="ConvertedValue">The converted value, rounded to 10 significant digits.</param>
/// <param name="ToUnit">Canonical name of the target unit.</param>
/// <param name="Category">Category both units belong to.</param>
public sealed record ConversionResponse(
    decimal OriginalValue,
    string FromUnit,
    decimal ConvertedValue,
    string ToUnit,
    UnitCategory Category);
