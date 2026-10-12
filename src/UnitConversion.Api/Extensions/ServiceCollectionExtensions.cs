using UnitConversion.Api.Services;
using UnitConversion.Api.Units;

namespace UnitConversion.Api.Extensions;

/// <summary>Dependency-injection registrations for the application.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the unit registry (built from <see cref="UnitCatalog"/>) and the conversion service.</summary>
    public static IServiceCollection AddUnitConversion(this IServiceCollection services)
    {
        // Both are stateless and immutable after construction, so a single shared instance is safe.
        services.AddSingleton<IUnitRegistry>(_ => new UnitRegistry(UnitCatalog.Categories, UnitCatalog.Units));
        services.AddSingleton<IUnitConversionService, UnitConversionService>();
        return services;
    }
}
