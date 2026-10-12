namespace UnitConversion.Api.Exceptions;

/// <summary>A unit name is missing or is not registered.</summary>
public sealed class UnsupportedUnitException : UnitConversionException
{
    /// <summary>Error code returned to API consumers.</summary>
    public const string Code = "unsupported_unit";

    /// <summary>Initializes the exception.</summary>
    /// <param name="parameterName">The request field that held the unit (<c>fromUnit</c> or <c>toUnit</c>).</param>
    /// <param name="unit">The rejected unit name, if any.</param>
    public UnsupportedUnitException(string parameterName, string? unit)
        : base(Code, BuildMessage(parameterName, unit))
    {
        ParameterName = parameterName;
        Unit = unit;
    }

    /// <summary>The request field that held the rejected unit.</summary>
    public string ParameterName { get; }

    /// <summary>The rejected unit name.</summary>
    public string? Unit { get; }

    private static string BuildMessage(string parameterName, string? unit) =>
        string.IsNullOrWhiteSpace(unit)
            ? $"'{parameterName}' must be a non-empty unit name."
            : $"Unit '{unit.Trim()}' (from '{parameterName}') is not supported.";
}
