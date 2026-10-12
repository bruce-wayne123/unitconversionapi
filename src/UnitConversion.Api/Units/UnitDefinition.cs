namespace UnitConversion.Api.Units;

/// <summary>
/// Describes one unit and how to move a value between that unit and its category's base unit.
/// </summary>
/// <remarks>
/// Every conversion is "source unit → base unit → target unit", so N units need only N pairs of
/// functions rather than N × N conversion rules. Proportional units (length, weight) are created with
/// <see cref="Linear"/>; units that need a formula (temperature) supply their own delegates.
/// </remarks>
public sealed class UnitDefinition
{
    private readonly Func<decimal, decimal> _toBase;
    private readonly Func<decimal, decimal> _fromBase;

    /// <summary>Creates a unit with custom conversion functions.</summary>
    /// <param name="name">Canonical (display) name, e.g. "Meter".</param>
    /// <param name="category">Category the unit belongs to.</param>
    /// <param name="toBase">Converts a value in this unit to the category's base unit.</param>
    /// <param name="fromBase">Converts a value in the category's base unit to this unit.</param>
    /// <param name="aliases">Additional accepted spellings, e.g. symbols such as "m".</param>
    public UnitDefinition(
        string name,
        UnitCategory category,
        Func<decimal, decimal> toBase,
        Func<decimal, decimal> fromBase,
        params string[] aliases)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(toBase);
        ArgumentNullException.ThrowIfNull(fromBase);

        Name = name.Trim();
        Category = category;
        Aliases = aliases;
        _toBase = toBase;
        _fromBase = fromBase;
    }

    /// <summary>Canonical (display) name of the unit.</summary>
    public string Name { get; }

    /// <summary>Category the unit belongs to.</summary>
    public UnitCategory Category { get; }

    /// <summary>Additional accepted spellings (symbols, alternative names).</summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>Converts a value in this unit to the category's base unit.</summary>
    public decimal ToBase(decimal value) => _toBase(value);

    /// <summary>Converts a value in the category's base unit to this unit.</summary>
    public decimal FromBase(decimal baseValue) => _fromBase(baseValue);

    /// <summary>
    /// Creates a proportional unit: <c>1 unit = <paramref name="baseUnitsPerUnit"/> base units</c>.
    /// </summary>
    /// <param name="name">Canonical (display) name.</param>
    /// <param name="category">Category the unit belongs to.</param>
    /// <param name="baseUnitsPerUnit">How many base units one of this unit equals. Must be positive.</param>
    /// <param name="aliases">Additional accepted spellings.</param>
    public static UnitDefinition Linear(
        string name,
        UnitCategory category,
        decimal baseUnitsPerUnit,
        params string[] aliases)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baseUnitsPerUnit);

        return new UnitDefinition(
            name,
            category,
            toBase: value => value * baseUnitsPerUnit,
            fromBase: baseValue => baseValue / baseUnitsPerUnit,
            aliases);
    }
}
