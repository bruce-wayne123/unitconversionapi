using System.Diagnostics.CodeAnalysis;

namespace UnitConversion.Api.Units;

/// <summary>
/// Central lookup for every supported unit and category.
/// </summary>
public interface IUnitRegistry
{
    /// <summary>All registered units.</summary>
    IReadOnlyCollection<UnitDefinition> Units { get; }

    /// <summary>
    /// Finds a unit by canonical name or alias (case-insensitive, surrounding whitespace ignored).
    /// </summary>
    /// <returns><c>true</c> if the unit is registered; otherwise <c>false</c>.</returns>
    bool TryGetUnit(string? name, [NotNullWhen(true)] out UnitDefinition? unit);

    /// <summary>Gets the metadata of a category.</summary>
    CategoryDefinition GetCategory(UnitCategory category);
}
