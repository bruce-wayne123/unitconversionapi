using UnitConversion.Api.Exceptions;
using UnitConversion.Api.Models;

namespace UnitConversion.Api.Services;

/// <summary>Converts values between units of the same category.</summary>
public interface IUnitConversionService
{
    /// <summary>Converts <paramref name="value"/> from one unit to another.</summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="fromUnit">Unit of the value (name or alias, case-insensitive).</param>
    /// <param name="toUnit">Unit to convert to (name or alias, case-insensitive).</param>
    /// <returns>The conversion result using canonical unit names.</returns>
    /// <exception cref="UnsupportedUnitException">A unit is missing or unknown.</exception>
    /// <exception cref="IncompatibleUnitCategoriesException">The units belong to different categories.</exception>
    /// <exception cref="InvalidConversionValueException">The value is not valid for the category or is too large.</exception>
    ConversionResponse Convert(decimal value, string fromUnit, string toUnit);
}
