using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnitConversion.Api.Controllers;
using UnitConversion.Api.Exceptions;
using UnitConversion.Api.Models;
using UnitConversion.Api.Services;
using UnitConversion.Api.Units;

namespace UnitConversion.UnitTests;

public class ConversionsControllerTests
{
    [Fact]
    public void Convert_ValidRequest_ReturnsOkWithServiceResult()
    {
        var controller = new ConversionsController(TestServices.CreateConversionService());
        var request = new ConversionRequest { Value = 100m, FromUnit = "Meter", ToUnit = "Foot" };

        var result = controller.Convert(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        var body = Assert.IsType<ConversionResponse>(ok.Value);
        Assert.Equal(328.0839895m, body.ConvertedValue, 7);
        Assert.Equal(UnitCategory.Length, body.Category);
    }

    [Fact]
    public void Convert_DelegatesRequestValuesToTheServiceAbstraction()
    {
        var fake = new RecordingConversionService();
        var controller = new ConversionsController(fake);

        controller.Convert(new ConversionRequest { Value = -12.5m, FromUnit = "a", ToUnit = "b" });

        Assert.Equal((-12.5m, "a", "b"), fake.LastCall);
    }

    [Fact]
    public void Convert_ServiceRejectsRequest_ExceptionPropagatesToGlobalHandler()
    {
        var controller = new ConversionsController(TestServices.CreateConversionService());
        var request = new ConversionRequest { Value = 1m, FromUnit = "Meter", ToUnit = "Celsius" };

        Assert.Throws<IncompatibleUnitCategoriesException>(() => { _ = controller.Convert(request); });
    }

    private sealed class RecordingConversionService : IUnitConversionService
    {
        public (decimal Value, string From, string To)? LastCall { get; private set; }

        public ConversionResponse Convert(decimal value, string fromUnit, string toUnit)
        {
            LastCall = (value, fromUnit, toUnit);
            return new ConversionResponse(value, fromUnit, value, toUnit, UnitCategory.Length);
        }
    }
}
