using System.Diagnostics.CodeAnalysis;

namespace UnitConversion.Api.Units;

/// <summary>
/// In-memory <see cref="IUnitRegistry"/>. Builds a case-insensitive dictionary once, so each lookup is O(1)
/// regardless of how many units are registered, and fails fast at start-up if the catalog is inconsistent.
/// </summary>
public sealed class UnitRegistry : IUnitRegistry
{
    private readonly Dictionary<string, UnitDefinition> _unitsByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<UnitCategory, CategoryDefinition> _categories = new();
    private readonly List<UnitDefinition> _units = [];

    /// <summary>Builds the registry and validates the catalog.</summary>
    /// <exception cref="InvalidOperationException">
    /// A name or alias is registered twice, a unit has no category definition, or a category's base unit is missing.
    /// </exception>
    public UnitRegistry(IEnumerable<CategoryDefinition> categories, IEnumerable<UnitDefinition> units)
    {
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(units);

        foreach (var category in categories)
        {
            if (!_categories.TryAdd(category.Category, category))
            {
                throw new InvalidOperationException($"Category '{category.Category}' is defined more than once.");
            }

            if (category.MinimumBaseValue is not null && string.IsNullOrWhiteSpace(category.MinimumValueDescription))
            {
                throw new InvalidOperationException(
                    $"Category '{category.Category}' has a minimum value but no description for it.");
            }
        }

        foreach (var unit in units)
        {
            if (!_categories.ContainsKey(unit.Category))
            {
                throw new InvalidOperationException(
                    $"Unit '{unit.Name}' belongs to category '{unit.Category}', which has no definition.");
            }

            Register(unit.Name, unit);
            foreach (var alias in unit.Aliases)
            {
                Register(alias, unit);
            }

            _units.Add(unit);
        }

        foreach (var category in _categories.Values)
        {
            if (!TryGetUnit(category.BaseUnitName, out var baseUnit) || baseUnit.Category != category.Category)
            {
                throw new InvalidOperationException(
                    $"Base unit '{category.BaseUnitName}' of category '{category.Category}' is not registered.");
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<UnitDefinition> Units => _units;

    /// <inheritdoc />
    public bool TryGetUnit(string? name, [NotNullWhen(true)] out UnitDefinition? unit)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            unit = null;
            return false;
        }

        return _unitsByKey.TryGetValue(name.Trim(), out unit);
    }

    /// <inheritdoc />
    public CategoryDefinition GetCategory(UnitCategory category) =>
        _categories.TryGetValue(category, out var definition)
            ? definition
            : throw new InvalidOperationException($"Category '{category}' is not registered.");

    private void Register(string key, UnitDefinition unit)
    {
        var trimmed = key.Trim();
        if (!_unitsByKey.TryAdd(trimmed, unit))
        {
            throw new InvalidOperationException(
                $"The unit name or alias '{trimmed}' is already used by '{_unitsByKey[trimmed].Name}'.");
        }
    }
}
