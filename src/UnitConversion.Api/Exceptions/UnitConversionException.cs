namespace UnitConversion.Api.Exceptions;

/// <summary>
/// Base type for errors caused by the caller's input. These are translated into HTTP 400 responses;
/// their messages are written for API consumers and are safe to expose.
/// </summary>
public abstract class UnitConversionException : Exception
{
    /// <summary>Initializes the exception.</summary>
    /// <param name="errorCode">Stable machine-readable code returned to API consumers.</param>
    /// <param name="message">Consumer-facing description of the problem.</param>
    protected UnitConversionException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>Stable machine-readable error code, e.g. <c>unsupported_unit</c>.</summary>
    public string ErrorCode { get; }
}
