using UnitConversion.Api.Exceptions;
using UnitConversion.Api.Services;
using UnitConversion.Api.Units;

namespace UnitConversion.UnitTests;

public class UnitConversionServiceTests
{
    private readonly UnitConversionService _service = TestServices.CreateConversionService();

    // Convenience: theory data is double because attribute arguments cannot be decimal.
    private decimal Convert(double value, string from, string to) =>
        _service.Convert((decimal)value, from, to).ConvertedValue;

    // ---------- Length ----------

    [Fact]
    public void Convert_MeterToFoot_ReturnsExpectedValueAndMetadata()
    {
        var result = _service.Convert(100m, "Meter", "Foot");

        Assert.Equal(328.0839895m, result.ConvertedValue, 7);
        Assert.Equal(100m, result.OriginalValue);
        Assert.Equal("Meter", result.FromUnit);
        Assert.Equal("Foot", result.ToUnit);
        Assert.Equal(UnitCategory.Length, result.Category);
    }

    [Theory]
    [InlineData(1.0, 1000.0)]
    [InlineData(2.5, 2500.0)]
    [InlineData(0.001, 1.0)]
    [InlineData(-3.0, -3000.0)]
    [InlineData(0.0, 0.0)]
    public void Convert_KilometerToMeter_ReturnsExpectedValue(double kilometers, double expectedMeters)
    {
        Assert.Equal((decimal)expectedMeters, Convert(kilometers, "Kilometer", "Meter"), 9);
    }

    [Theory]
    [InlineData(1.0, "Mile", "Kilometer", 1.609344)]
    [InlineData(1.0, "Inch", "Centimeter", 2.54)]
    [InlineData(1.0, "Yard", "Foot", 3.0)]
    [InlineData(1.0, "Foot", "Inch", 12.0)]
    [InlineData(1.0, "Meter", "Millimeter", 1000.0)]
    [InlineData(1.0, "Centimeter", "Millimeter", 10.0)]
    public void Convert_OtherLengthPairs_ReturnExpectedValue(double value, string from, string to, double expected)
    {
        Assert.Equal((decimal)expected, Convert(value, from, to), 9);
    }

    [Fact]
    public void Convert_NegativeLength_IsAllowed()
    {
        // Negative lengths are valid as signed displacement.
        Assert.Equal(-16.40419948m, _service.Convert(-5m, "Meter", "Foot").ConvertedValue, 8);
    }

    // ---------- Temperature ----------

    [Theory]
    [InlineData(100.0, 212.0)]
    [InlineData(0.0, 32.0)]
    [InlineData(-40.0, -40.0)]
    [InlineData(37.0, 98.6)]
    [InlineData(36.6, 97.88)]
    [InlineData(-273.15, -459.67)]
    public void Convert_CelsiusToFahrenheit_ReturnsExpectedValue(double celsius, double expectedFahrenheit)
    {
        Assert.Equal((decimal)expectedFahrenheit, Convert(celsius, "Celsius", "Fahrenheit"), 8);
    }

    [Theory]
    [InlineData(212.0, 100.0)]
    [InlineData(32.0, 0.0)]
    [InlineData(-40.0, -40.0)]
    [InlineData(98.6, 37.0)]
    [InlineData(-459.67, -273.15)]
    [InlineData(0.0, -17.77777778)]
    public void Convert_FahrenheitToCelsius_ReturnsExpectedValue(double fahrenheit, double expectedCelsius)
    {
        Assert.Equal((decimal)expectedCelsius, Convert(fahrenheit, "Fahrenheit", "Celsius"), 8);
    }

    [Theory]
    [InlineData(0.0, 273.15)]
    [InlineData(100.0, 373.15)]
    [InlineData(25.0, 298.15)]
    [InlineData(-40.0, 233.15)]
    [InlineData(-273.15, 0.0)]
    public void Convert_CelsiusToKelvin_ReturnsExpectedValue(double celsius, double expectedKelvin)
    {
        Assert.Equal((decimal)expectedKelvin, Convert(celsius, "Celsius", "Kelvin"), 8);
    }

    [Theory]
    [InlineData(0.0, -273.15)]
    [InlineData(300.0, 26.85)]
    public void Convert_KelvinToCelsius_ReturnsExpectedValue(double kelvin, double expectedCelsius)
    {
        Assert.Equal((decimal)expectedCelsius, Convert(kelvin, "Kelvin", "Celsius"), 8);
    }

    [Fact]
    public void Convert_KelvinToFahrenheit_ReturnsExpectedValue()
    {
        Assert.Equal(80.33m, _service.Convert(300m, "Kelvin", "Fahrenheit").ConvertedValue, 8);
    }

    // ---------- Temperature boundaries (absolute zero) ----------

    [Fact]
    public void Convert_AbsoluteZeroKelvin_MapsToCelsiusAndFahrenheitAbsoluteZero()
    {
        Assert.Equal(-273.15m, _service.Convert(0m, "Kelvin", "Celsius").ConvertedValue);
        Assert.Equal(-459.67m, _service.Convert(0m, "Kelvin", "Fahrenheit").ConvertedValue);
    }

    [Fact]
    public void Convert_AbsoluteZeroInCelsiusAndFahrenheit_MapsToZeroKelvin()
    {
        Assert.Equal(0m, _service.Convert(-273.15m, "Celsius", "Kelvin").ConvertedValue);
        Assert.Equal(0m, _service.Convert(-459.67m, "Fahrenheit", "Kelvin").ConvertedValue);
    }

    [Fact]
    public void Convert_AbsoluteZeroBetweenCelsiusAndFahrenheit_IsConsistent()
    {
        Assert.Equal(-459.67m, _service.Convert(-273.15m, "Celsius", "Fahrenheit").ConvertedValue);
        Assert.Equal(-273.15m, _service.Convert(-459.67m, "Fahrenheit", "Celsius").ConvertedValue);
    }

    [Theory]
    [InlineData("Kelvin", -0.01)]
    [InlineData("Celsius", -273.16)]
    [InlineData("Fahrenheit", -459.68)]
    public void Convert_TemperatureBelowAbsoluteZero_ThrowsInvalidConversionValue(string unit, double value)
    {
        var exception = Assert.Throws<InvalidConversionValueException>(
            () => { _ = _service.Convert((decimal)value, unit, "Kelvin"); });

        Assert.Equal("invalid_value", exception.ErrorCode);
        Assert.Contains("absolute zero", exception.Message);
    }

    // ---------- Weight ----------

    [Theory]
    [InlineData(1.0, 2.204622622)]
    [InlineData(10.0, 22.04622622)]
    [InlineData(0.0, 0.0)]
    public void Convert_KilogramToPound_ReturnsExpectedValue(double kilograms, double expectedPounds)
    {
        Assert.Equal((decimal)expectedPounds, Convert(kilograms, "Kilogram", "Pound"), 8);
    }

    [Theory]
    [InlineData(1500.0, 1.5)]
    [InlineData(1.0, 0.001)]
    [InlineData(250.0, 0.25)]
    [InlineData(0.5, 0.0005)]
    public void Convert_GramToKilogram_ReturnsExpectedValue(double grams, double expectedKilograms)
    {
        Assert.Equal((decimal)expectedKilograms, Convert(grams, "Gram", "Kilogram"), 9);
    }

    [Theory]
    [InlineData(1.0, "Pound", "Kilogram", 0.45359237)]
    [InlineData(1.0, "Ounce", "Gram", 28.34952313)]
    [InlineData(2.5, "Metric Ton", "Kilogram", 2500.0)]
    [InlineData(1000.0, "Milligram", "Gram", 1.0)]
    public void Convert_OtherWeightPairs_ReturnExpectedValue(double value, string from, string to, double expected)
    {
        Assert.Equal((decimal)expected, Convert(value, from, to), 8);
    }

    [Fact]
    public void Convert_VerySmallResult_IsNotRoundedToZero()
    {
        // 1 mg = 1e-9 metric tons; fixed-decimal rounding would have returned 0.
        Assert.Equal(0.000000001m, _service.Convert(1m, "Milligram", "Metric Ton").ConvertedValue);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-0.001)]
    public void Convert_NegativeWeight_ThrowsInvalidConversionValue(double value)
    {
        Assert.Throws<InvalidConversionValueException>(
            () => { _ = _service.Convert((decimal)value, "Kilogram", "Pound"); });
    }

    // ---------- Case-insensitivity and aliases ----------

    [Theory]
    [InlineData("meter", "foot")]
    [InlineData("METER", "FOOT")]
    [InlineData("mEtEr", "fOoT")]
    [InlineData("m", "ft")]
    [InlineData("metre", "feet")]
    [InlineData("  Meter  ", "  Foot ")]
    public void Convert_UnitNamesAreCaseInsensitive_AndReturnCanonicalNames(string from, string to)
    {
        var result = _service.Convert(100m, from, to);

        Assert.Equal(328.0839895m, result.ConvertedValue, 7);
        Assert.Equal("Meter", result.FromUnit);
        Assert.Equal("Foot", result.ToUnit);
    }

    [Theory]
    [InlineData("celsius", "FAHRENHEIT")]
    [InlineData("°c", "°F")]
    [InlineData("C", "f")]
    public void Convert_TemperatureUnitNames_AreCaseInsensitive(string from, string to)
    {
        Assert.Equal(212m, _service.Convert(100m, from, to).ConvertedValue);
    }

    [Theory]
    [InlineData("metric ton")]
    [InlineData("METRIC TON")]
    [InlineData("MetricTon")]
    [InlineData("tonne")]
    public void Convert_MetricTonSpellings_AreAccepted(string name)
    {
        var result = _service.Convert(1m, name, "kg");

        Assert.Equal("Metric Ton", result.FromUnit);
        Assert.Equal(1000m, result.ConvertedValue);
    }

    // ---------- Unsupported units ----------

    [Fact]
    public void Convert_UnsupportedSourceUnit_ThrowsWithFieldAndUnit()
    {
        var exception = Assert.Throws<UnsupportedUnitException>(
            () => { _ = _service.Convert(1m, "Furlong", "Meter"); });

        Assert.Equal("fromUnit", exception.ParameterName);
        Assert.Equal("Furlong", exception.Unit);
        Assert.Equal("unsupported_unit", exception.ErrorCode);
        Assert.Contains("Furlong", exception.Message);
    }

    [Fact]
    public void Convert_UnsupportedTargetUnit_ThrowsWithFieldAndUnit()
    {
        var exception = Assert.Throws<UnsupportedUnitException>(
            () => { _ = _service.Convert(1m, "Meter", "Parsec"); });

        Assert.Equal("toUnit", exception.ParameterName);
        Assert.Contains("Parsec", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Convert_MissingUnitName_ThrowsUnsupportedUnit(string? unit)
    {
        var exception = Assert.Throws<UnsupportedUnitException>(
            () => { _ = _service.Convert(1m, unit!, "Meter"); });

        Assert.Contains("non-empty", exception.Message);
    }

    [Fact]
    public void Convert_UnsupportedUnitIsReportedBeforeCategoryMismatch()
    {
        Assert.Throws<UnsupportedUnitException>(
            () => { _ = _service.Convert(1m, "Meter", "Kilobanana"); });
    }

    // ---------- Incompatible categories ----------

    [Theory]
    [InlineData("Meter", "Celsius")]
    [InlineData("Kilogram", "Foot")]
    [InlineData("Kelvin", "Gram")]
    public void Convert_UnitsFromDifferentCategories_ThrowsIncompatibleCategories(string from, string to)
    {
        var exception = Assert.Throws<IncompatibleUnitCategoriesException>(
            () => { _ = _service.Convert(1m, from, to); });

        Assert.Equal("incompatible_categories", exception.ErrorCode);
    }

    [Fact]
    public void Convert_IncompatibleCategories_MessageNamesBothCategories()
    {
        var exception = Assert.Throws<IncompatibleUnitCategoriesException>(
            () => { _ = _service.Convert(1m, "Meter", "Celsius"); });

        Assert.Contains("Length", exception.Message);
        Assert.Contains("Temperature", exception.Message);
    }

    // ---------- Invalid values ----------

    [Fact]
    public void Convert_ValueTooLargeToRepresent_ThrowsInvalidConversionValue()
    {
        var exception = Assert.Throws<InvalidConversionValueException>(
            () => { _ = _service.Convert(decimal.MaxValue, "Kilometer", "Millimeter"); });

        Assert.Contains("too large", exception.Message);
    }

    // ---------- General behaviour ----------

    [Theory]
    [InlineData(42.5, "Meter", "Meter")]
    [InlineData(25.0, "Celsius", "Celsius")]
    [InlineData(3.0, "Pound", "Pound")]
    public void Convert_SameUnit_ReturnsOriginalValue(double value, string from, string to)
    {
        Assert.Equal((decimal)value, Convert(value, from, to), 9);
    }

    [Theory]
    [InlineData("Meter", "Foot")]
    [InlineData("Mile", "Yard")]
    [InlineData("Celsius", "Fahrenheit")]
    [InlineData("Kelvin", "Celsius")]
    [InlineData("Kilogram", "Pound")]
    [InlineData("Ounce", "Metric Ton")]
    public void Convert_RoundTrip_ReturnsOriginalValue(string a, string b)
    {
        const decimal original = 123.456m;

        var there = _service.Convert(original, a, b).ConvertedValue;
        var back = _service.Convert(there, b, a).ConvertedValue;

        Assert.Equal(original, back, 5);
    }

    [Fact]
    public void Convert_ExtendedRegistry_SupportsNewUnitWithoutServiceChanges()
    {
        var units = UnitCatalog.Units.Append(
            UnitDefinition.Linear("Nautical Mile", UnitCategory.Length, 1852m, "nmi")).ToList();
        var service = new UnitConversionService(new UnitRegistry(UnitCatalog.Categories, units));

        Assert.Equal(1852m, service.Convert(1m, "nmi", "Meter").ConvertedValue);
        Assert.Equal(1m, service.Convert(1852m, "Meter", "Nautical Mile").ConvertedValue);
        Assert.Equal(1.15077945m, service.Convert(1m, "Nautical Mile", "Mile").ConvertedValue, 8);
    }
}
