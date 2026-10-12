using System.Globalization;
using UnitConversion.Api.Services;

namespace UnitConversion.UnitTests;

public class DecimalRoundingTests
{
    [Fact]
    public void ToSignificantDigits_LongFraction_KeepsTenSignificantDigits()
    {
        Assert.Equal(328.0839895m, DecimalRounding.ToSignificantDigits(328.083989501312335958m));
    }

    [Fact]
    public void ToSignificantDigits_VerySmallValue_IsNotRoundedAway()
    {
        Assert.Equal(0.00000000123456789m, DecimalRounding.ToSignificantDigits(0.000000001234567890123m));
    }

    [Fact]
    public void ToSignificantDigits_LargeValue_RoundsLeftOfDecimalPoint()
    {
        Assert.Equal(12345678900000m, DecimalRounding.ToSignificantDigits(12345678901234m));
    }

    [Fact]
    public void ToSignificantDigits_MidpointRoundsAwayFromZero()
    {
        Assert.Equal(0.1234567891m, DecimalRounding.ToSignificantDigits(0.12345678905m));
        Assert.Equal(-0.1234567891m, DecimalRounding.ToSignificantDigits(-0.12345678905m));
    }

    [Fact]
    public void ToSignificantDigits_RoundingUpAcrossPowerOfTen_IsHandled()
    {
        Assert.Equal(1000000m, DecimalRounding.ToSignificantDigits(999999.99999999999m));
    }

    [Fact]
    public void ToSignificantDigits_Zero_ReturnsZero()
    {
        Assert.Equal(0m, DecimalRounding.ToSignificantDigits(0m));
    }

    [Fact]
    public void ToSignificantDigits_RemovesTrailingZeros()
    {
        var result = DecimalRounding.ToSignificantDigits(3.50000000000001m);

        Assert.Equal("3.5", result.ToString(CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    public void ToSignificantDigits_InvalidDigitCount_Throws(int digits)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => { _ = DecimalRounding.ToSignificantDigits(1m, digits); });
    }
}
