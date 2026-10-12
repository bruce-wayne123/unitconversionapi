namespace UnitConversion.Api.Units;

/// <summary>
/// The kinds of quantity the API can convert. Units can only be converted within the same category.
/// </summary>
public enum UnitCategory
{
    /// <summary>Distance (meter, foot, mile, ...).</summary>
    Length,

    /// <summary>Temperature (Celsius, Fahrenheit, Kelvin).</summary>
    Temperature,

    /// <summary>Weight / mass (kilogram, pound, ...).</summary>
    Weight
}
