using UnitConversion.Api.Services;
using UnitConversion.Api.Units;

namespace UnitConversion.UnitTests;

/// <summary>Builds the real (non-mocked) service graph used by most tests.</summary>
internal static class TestServices
{
    public static UnitRegistry CreateRegistry() => new(UnitCatalog.Categories, UnitCatalog.Units);

    public static UnitConversionService CreateConversionService() => new(CreateRegistry());
}
