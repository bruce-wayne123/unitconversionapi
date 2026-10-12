using UnitConversion.Api.Units;

namespace UnitConversion.Api.Exceptions;

/// <summary>The source and target units belong to different categories (e.g. Meter to Celsius).</summary>
public sealed class IncompatibleUnitCategoriesException : UnitConversionException
{
    /// <summary>Error code returned to API consumers.</summary>
    public const string Code = "incompatible_categories";

    /// <summary>Initializes the exception.</summary>
    /// <param name="source">The source unit.</param>
    /// <param name="target">The target unit.</param>
    public IncompatibleUnitCategoriesException(UnitDefinition source, UnitDefinition target)
        : base(Code,
            $"Cannot convert {source.Name} ({source.Category}) to {target.Name} ({target.Category}): " +
            "units must belong to the same category.")
    {
    }
}
