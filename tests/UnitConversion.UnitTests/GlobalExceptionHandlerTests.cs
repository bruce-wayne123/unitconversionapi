using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using UnitConversion.Api.Exceptions;
using UnitConversion.Api.Units;

namespace UnitConversion.UnitTests;

public class GlobalExceptionHandlerTests
{
    private static async Task<(DefaultHttpContext Context, JsonElement Body, string RawBody)> HandleAsync(
        Exception exception, string? acceptHeader = null)
    {
        var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        if (acceptHeader is not null)
        {
            context.Request.Headers.Accept = acceptHeader;
        }

        var handler = new GlobalExceptionHandler(
            services.GetRequiredService<IProblemDetailsService>(),
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        Assert.True(handled);

        context.Response.Body.Position = 0;
        var raw = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context, JsonDocument.Parse(raw).RootElement.Clone(), raw);
    }

    [Fact]
    public async Task TryHandleAsync_ConversionException_Returns400ProblemDetailsWithCode()
    {
        var exception = new UnsupportedUnitException("fromUnit", "Furlong");

        var (context, body, _) = await HandleAsync(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("problem+json", context.Response.ContentType!);
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Equal("unsupported_unit", body.GetProperty("code").GetString());
        Assert.Contains("Furlong", body.GetProperty("detail").GetString()!);
    }

    [Fact]
    public async Task TryHandleAsync_IncompatibleCategories_ReturnsItsOwnErrorCode()
    {
        var registry = TestServices.CreateRegistry();
        registry.TryGetUnit("Meter", out var meter);
        registry.TryGetUnit("Celsius", out var celsius);

        var (context, body, _) = await HandleAsync(new IncompatibleUnitCategoriesException(meter!, celsius!));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("incompatible_categories", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Returns500WithoutLeakingDetails()
    {
        var exception = new InvalidOperationException("secret connection string = 12345");

        var (context, body, raw) = await HandleAsync(exception);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal(500, body.GetProperty("status").GetInt32());
        Assert.DoesNotContain("secret", raw);
        Assert.DoesNotContain("InvalidOperationException", raw);
        Assert.DoesNotContain("StackTrace", raw);
    }

    [Fact]
    public async Task TryHandleAsync_ClientDoesNotAcceptProblemJson_StillReturnsJsonBody()
    {
        var (context, body, _) = await HandleAsync(
            new InvalidConversionValueException("bad value"), acceptHeader: "text/html");

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("bad value", body.GetProperty("detail").GetString());
    }
}
