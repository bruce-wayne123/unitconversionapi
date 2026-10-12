using System.Globalization;
using UnitConversion.Api.Exceptions;
using UnitConversion.Api.Models;
using UnitConversion.Api.Units;

namespace UnitConversion.Api.Services;

/// <summary>
/// Converts values by routing every conversion through the category's base unit:
/// <c>source → base → target</c>. The service contains no per-unit logic; all unit knowledge lives in the
/// <see cref="IUnitRegistry"/>.
/// </summary>
/// <remarks>
/// The work is pure in-memory arithmetic, so the methods are synchronous; async would add overhead with no benefit.
/// </remarks>
public sealed class UnitConversionService(IUnitRegistry registry) : IUnitConversionService
{
    private const string FromUnitField = "fromUnit";
    private const string ToUnitField = "toUnit";

    /// <inheritdoc />
    public ConversionResponse Convert(decimal value, string fromUnit, string toUnit)
    {
        var source = ResolveUnit(fromUnit, FromUnitField);
        var target = ResolveUnit(toUnit, ToUnitField);

        if (source.Category != target.Category)
        {
            throw new IncompatibleUnitCategoriesException(source, target);
        }

        try
        {
            var baseValue = source.ToBase(value);
            EnsureWithinCategoryLimits(value, source, baseValue);

            var converted = DecimalRounding.ToSignificantDigits(target.FromBase(baseValue));

            return new ConversionResponse(value, source.Name, converted, target.Name, source.Category);
        }
        catch (OverflowException)
        {
            throw new InvalidConversionValueException(
                $"The value is too large to convert from {source.Name} to {target.Name}.");
        }
    }

    private UnitDefinition ResolveUnit(string? name, string fieldName) =>
        registry.TryGetUnit(name, out var unit)
            ? unit
            : throw new UnsupportedUnitException(fieldName, name);

    /// <summary>Rejects physically impossible values, such as temperatures below absolute zero.</summary>
    private void EnsureWithinCategoryLimits(decimal value, UnitDefinition source, decimal baseValue)
    {
        var category = registry.GetCategory(source.Category);

        if (category.MinimumBaseValue is { } minimum && baseValue < minimum)
        {
            var shownValue = value.ToString(CultureInfo.InvariantCulture);
            throw new InvalidConversionValueException(
                $"{shownValue} {source.Name} is not a valid {source.Category} value: " +
                $"it is below {category.MinimumValueDescription}.");
        }
    }
}
