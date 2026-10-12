namespace UnitConversion.Api.Exceptions;

/// <summary>The numeric value cannot be converted (outside the valid range or too large).</summary>
public sealed class InvalidConversionValueException : UnitConversionException
{
    /// <summary>Error code returned to API consumers.</summary>
    public const string Code = "invalid_value";

    /// <summary>Initializes the exception.</summary>
    /// <param name="message">Consumer-facing description of the problem.</param>
    public InvalidConversionValueException(string message)
        : base(Code, message)
    {
    }
}
