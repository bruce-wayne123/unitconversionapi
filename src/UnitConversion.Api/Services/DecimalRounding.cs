using System.Globalization;

namespace UnitConversion.Api.Services;

/// <summary>Rounding helpers that keep results readable without discarding very small values.</summary>
public static class DecimalRounding
{
    /// <summary>Number of significant digits kept in conversion results.</summary>
    public const int DefaultSignificantDigits = 10;

    private const int MaxSignificantDigits = 28;   // precision of System.Decimal
    private const int MaxDecimalScale = 28;

    // Dividing by a decimal with 28 trailing zeros removes the trailing zeros of the dividend (3.50000 -> 3.5).
    private const decimal TrailingZeroTrimmer = 1.0000000000000000000000000000m;

    /// <summary>
    /// Rounds to a number of <em>significant</em> digits (not decimal places), so 0.000000001 stays
    /// non-zero while 328.08398950131 becomes 328.0839895. Trailing zeros are removed.
    /// </summary>
    /// <exception cref="OverflowException">Rounding up pushes the value past the range of <see cref="decimal"/>.</exception>
    public static decimal ToSignificantDigits(decimal value, int significantDigits = DefaultSignificantDigits)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(significantDigits, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(significantDigits, MaxSignificantDigits);

        if (value == 0m)
        {
            return 0m;
        }

        var decimals = significantDigits - GetMagnitude(Math.Abs(value));

        decimal rounded;
        if (decimals >= 0)
        {
            rounded = Math.Round(value, Math.Min(decimals, MaxDecimalScale), MidpointRounding.AwayFromZero);
        }
        else
        {
            // Large numbers: round to the left of the decimal point (e.g. to the nearest 1000).
            var factor = PowerOfTen(-decimals);
            rounded = Math.Round(value / factor, 0, MidpointRounding.AwayFromZero) * factor;
        }

        return rounded == 0m ? 0m : rounded / TrailingZeroTrimmer;
    }

    /// <summary>
    /// Position of the first significant digit relative to the decimal point:
    /// 328.08 → 3, 0.5 → 0, 0.05 → -1.
    /// </summary>
    private static int GetMagnitude(decimal absoluteValue)
    {
        if (absoluteValue >= 1m)
        {
            return Math.Truncate(absoluteValue).ToString("F0", CultureInfo.InvariantCulture).Length;
        }

        var magnitude = 0;
        while (absoluteValue < 0.1m)
        {
            absoluteValue *= 10m;
            magnitude--;
        }

        return magnitude;
    }

    private static decimal PowerOfTen(int exponent)
    {
        var result = 1m;
        for (var i = 0; i < exponent; i++)
        {
            result *= 10m;
        }

        return result;
    }
}
