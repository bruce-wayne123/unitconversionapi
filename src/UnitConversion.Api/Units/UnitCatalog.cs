namespace UnitConversion.Api.Units;

/// <summary>
/// The built-in data set: every supported category and unit, plus the named conversion constants.
/// </summary>
/// <remarks>
/// To support a new unit, add one line to <see cref="CreateUnits"/>. To support a new category, add a value to
/// <see cref="UnitCategory"/>, a <see cref="CategoryDefinition"/> and its units. No conversion code changes.
/// </remarks>
public static class UnitCatalog
{
    // ---- Length: base unit is the meter (all factors are exact by international definition) ----
    private const decimal MetersPerKilometer = 1000m;
    private const decimal MetersPerCentimeter = 0.01m;
    private const decimal MetersPerMillimeter = 0.001m;
    private const decimal MetersPerInch = 0.0254m;
    private const decimal MetersPerFoot = 0.3048m;
    private const decimal MetersPerYard = 0.9144m;
    private const decimal MetersPerMile = 1609.344m;

    // ---- Weight / mass: base unit is the kilogram ----
    private const decimal KilogramsPerGram = 0.001m;
    private const decimal KilogramsPerMilligram = 0.000001m;
    private const decimal KilogramsPerMetricTon = 1000m;
    private const decimal KilogramsPerPound = 0.45359237m;      // exact by international definition
    private const decimal KilogramsPerOunce = 0.028349523125m;  // 1/16 pound, exact

    // ---- Temperature: base unit is the kelvin ----
    private const decimal CelsiusKelvinOffset = 273.15m;        // 0 °C = 273.15 K
    private const decimal FahrenheitKelvinOffset = 459.67m;     // 0 K = -459.67 °F
    private const decimal FahrenheitStepsPerRatio = 9m;         // 9 °F steps ...
    private const decimal KelvinStepsPerRatio = 5m;             // ... equal 5 K steps

    /// <summary>Category metadata for all built-in categories.</summary>
    public static IReadOnlyList<CategoryDefinition> Categories { get; } =
    [
        // Negative lengths are valid (signed displacement along an axis), so there is no minimum.
        new(UnitCategory.Length, BaseUnitName: "Meter"),
        new(UnitCategory.Temperature, BaseUnitName: "Kelvin",
            MinimumBaseValue: 0m, MinimumValueDescription: "absolute zero (0 K)"),
        new(UnitCategory.Weight, BaseUnitName: "Kilogram",
            MinimumBaseValue: 0m, MinimumValueDescription: "zero (weight cannot be negative)")
    ];

    /// <summary>All built-in units.</summary>
    public static IReadOnlyList<UnitDefinition> Units { get; } = CreateUnits();

    private static UnitDefinition[] CreateUnits() =>
    [
        // Length
        UnitDefinition.Linear("Meter", UnitCategory.Length, 1m, "m", "Metre"),
        UnitDefinition.Linear("Kilometer", UnitCategory.Length, MetersPerKilometer, "km", "Kilometre"),
        UnitDefinition.Linear("Centimeter", UnitCategory.Length, MetersPerCentimeter, "cm", "Centimetre"),
        UnitDefinition.Linear("Millimeter", UnitCategory.Length, MetersPerMillimeter, "mm", "Millimetre"),
        UnitDefinition.Linear("Mile", UnitCategory.Length, MetersPerMile, "mi"),
        UnitDefinition.Linear("Yard", UnitCategory.Length, MetersPerYard, "yd"),
        UnitDefinition.Linear("Foot", UnitCategory.Length, MetersPerFoot, "ft", "Feet"),
        UnitDefinition.Linear("Inch", UnitCategory.Length, MetersPerInch, "in", "Inches"),

        // Weight / mass
        UnitDefinition.Linear("Kilogram", UnitCategory.Weight, 1m, "kg"),
        UnitDefinition.Linear("Gram", UnitCategory.Weight, KilogramsPerGram, "g"),
        UnitDefinition.Linear("Milligram", UnitCategory.Weight, KilogramsPerMilligram, "mg"),
        UnitDefinition.Linear("Metric Ton", UnitCategory.Weight, KilogramsPerMetricTon, "MetricTon", "Tonne", "t"),
        UnitDefinition.Linear("Pound", UnitCategory.Weight, KilogramsPerPound, "lb", "lbs"),
        UnitDefinition.Linear("Ounce", UnitCategory.Weight, KilogramsPerOunce, "oz"),

        // Temperature: explicit formulas through Kelvin (an offset is needed, so a plain factor is not enough).
        new UnitDefinition(
            "Kelvin", UnitCategory.Temperature,
            toBase: kelvin => kelvin,
            fromBase: kelvin => kelvin,
            "K"),
        new UnitDefinition(
            "Celsius", UnitCategory.Temperature,
            toBase: celsius => celsius + CelsiusKelvinOffset,
            fromBase: kelvin => kelvin - CelsiusKelvinOffset,
            "C", "°C"),
        // Multiply before dividing so the 5/9 ratio does not lose precision.
        new UnitDefinition(
            "Fahrenheit", UnitCategory.Temperature,
            toBase: fahrenheit => (fahrenheit + FahrenheitKelvinOffset) * KelvinStepsPerRatio / FahrenheitStepsPerRatio,
            fromBase: kelvin => kelvin * FahrenheitStepsPerRatio / KelvinStepsPerRatio - FahrenheitKelvinOffset,
            "F", "°F")
    ];
}
