using System.ComponentModel.DataAnnotations;
using UnitConversion.Api.Models;

namespace UnitConversion.UnitTests;

/// <summary>Verifies the DataAnnotations that [ApiController] uses to return 400 for malformed requests.</summary>
public class ConversionRequestValidationTests
{
    private static List<ValidationResult> Validate(ConversionRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Validate_CompleteRequest_HasNoErrors()
    {
        var request = new ConversionRequest { Value = 100m, FromUnit = "Meter", ToUnit = "Foot" };

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void Validate_MissingValue_ReportsValueError()
    {
        var request = new ConversionRequest { FromUnit = "Meter", ToUnit = "Foot" };

        var error = Assert.Single(Validate(request));
        Assert.Contains("'value'", error.ErrorMessage!);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankFromUnit_ReportsFromUnitError(string unit)
    {
        var request = new ConversionRequest { Value = 1m, FromUnit = unit, ToUnit = "Foot" };

        var error = Assert.Single(Validate(request));
        Assert.Contains("'fromUnit'", error.ErrorMessage!);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankToUnit_ReportsToUnitError(string unit)
    {
        var request = new ConversionRequest { Value = 1m, FromUnit = "Meter", ToUnit = unit };

        var error = Assert.Single(Validate(request));
        Assert.Contains("'toUnit'", error.ErrorMessage!);
    }

    [Fact]
    public void Validate_OverlongUnitName_ReportsError()
    {
        var request = new ConversionRequest
        {
            Value = 1m,
            FromUnit = new string('x', ConversionRequest.MaxUnitNameLength + 1),
            ToUnit = "Foot"
        };

        var error = Assert.Single(Validate(request));
        Assert.Contains("at most", error.ErrorMessage!);
    }

    [Fact]
    public void Validate_EverythingMissing_ReportsAllThreeFields()
    {
        Assert.Equal(3, Validate(new ConversionRequest()).Count);
    }
}
