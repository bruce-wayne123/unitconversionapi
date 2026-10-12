using UnitConversion.Api.Units;

namespace UnitConversion.UnitTests;

public class UnitRegistryTests
{
    private readonly UnitRegistry _registry = TestServices.CreateRegistry();

    [Theory]
    [InlineData("Meter", "Meter")]
    [InlineData("meter", "Meter")]
    [InlineData("KM", "Kilometer")]
    [InlineData("  ft ", "Foot")]
    [InlineData("°c", "Celsius")]
    [InlineData("lbs", "Pound")]
    [InlineData("metric ton", "Metric Ton")]
    public void TryGetUnit_KnownNameOrAlias_ReturnsCanonicalUnit(string input, string expectedName)
    {
        Assert.True(_registry.TryGetUnit(input, out var unit));
        Assert.Equal(expectedName, unit!.Name);
    }

    [Theory]
    [InlineData("Furlong")]
    [InlineData("Met er")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryGetUnit_UnknownOrBlank_ReturnsFalse(string? input)
    {
        Assert.False(_registry.TryGetUnit(input, out var unit));
        Assert.Null(unit);
    }

    [Fact]
    public void Constructor_BuiltInCatalog_IsConsistentAndComplete()
    {
        Assert.Equal(17, _registry.Units.Count);

        foreach (var unit in UnitCatalog.Units)
        {
            Assert.True(_registry.TryGetUnit(unit.Name, out var byName));
            Assert.Same(unit, byName);

            foreach (var alias in unit.Aliases)
            {
                Assert.True(_registry.TryGetUnit(alias, out var byAlias));
                Assert.Same(unit, byAlias);
            }
        }
    }

    [Fact]
    public void GetCategory_Temperature_HasAbsoluteZeroMinimum()
    {
        var category = _registry.GetCategory(UnitCategory.Temperature);

        Assert.Equal("Kelvin", category.BaseUnitName);
        Assert.Equal(0m, category.MinimumBaseValue);
    }

    [Fact]
    public void Constructor_DuplicateNameIgnoringCase_Throws()
    {
        var units = new[]
        {
            UnitDefinition.Linear("Meter", UnitCategory.Length, 1m),
            UnitDefinition.Linear("METER", UnitCategory.Length, 2m)
        };

        var exception = Assert.Throws<InvalidOperationException>(
            () => { _ = new UnitRegistry(UnitCatalog.Categories, units); });

        Assert.Contains("already used", exception.Message);
    }

    [Fact]
    public void Constructor_AliasCollidingWithAnotherUnit_Throws()
    {
        var units = new[]
        {
            UnitDefinition.Linear("Meter", UnitCategory.Length, 1m, "m"),
            UnitDefinition.Linear("Mile", UnitCategory.Length, 1609.344m, "M")
        };

        Assert.Throws<InvalidOperationException>(
            () => { _ = new UnitRegistry(UnitCatalog.Categories, units); });
    }

    [Fact]
    public void Constructor_UnitWithoutCategoryDefinition_Throws()
    {
        var categories = new[] { new CategoryDefinition(UnitCategory.Length, "Meter") };
        var units = new[]
        {
            UnitDefinition.Linear("Meter", UnitCategory.Length, 1m),
            UnitDefinition.Linear("Gram", UnitCategory.Weight, 0.001m)
        };

        Assert.Throws<InvalidOperationException>(
            () => { _ = new UnitRegistry(categories, units); });
    }

    [Fact]
    public void Constructor_MissingBaseUnit_Throws()
    {
        var categories = new[] { new CategoryDefinition(UnitCategory.Length, "Meter") };
        var units = new[] { UnitDefinition.Linear("Foot", UnitCategory.Length, 0.3048m) };

        var exception = Assert.Throws<InvalidOperationException>(
            () => { _ = new UnitRegistry(categories, units); });

        Assert.Contains("Base unit", exception.Message);
    }

    [Fact]
    public void Linear_NonPositiveFactor_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => { _ = UnitDefinition.Linear("Broken", UnitCategory.Length, 0m); });
    }
}
