namespace UnitConversion.Api.Units;

/// <summary>
/// Metadata shared by every unit of one <see cref="UnitCategory"/>.
/// </summary>
/// <param name="Category">The category being described.</param>
/// <param name="BaseUnitName">
/// Canonical name of the unit every other unit in the category converts through
/// (for example Meter for length, Kelvin for temperature).
/// </param>
/// <param name="MinimumBaseValue">
/// Lowest physically valid value, expressed in the base unit, or <c>null</c> when any value is valid.
/// </param>
/// <param name="MinimumValueDescription">
/// Human-readable explanation of the minimum (used in error messages). Required when
/// <paramref name="MinimumBaseValue"/> is set.
/// </param>
public sealed record CategoryDefinition(
    UnitCategory Category,
    string BaseUnitName,
    decimal? MinimumBaseValue = null,
    string? MinimumValueDescription = null);
