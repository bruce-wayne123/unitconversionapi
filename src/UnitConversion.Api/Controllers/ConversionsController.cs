using Microsoft.AspNetCore.Mvc;
using UnitConversion.Api.Models;
using UnitConversion.Api.Services;

namespace UnitConversion.Api.Controllers;

/// <summary>Unit conversion endpoints.</summary>
[ApiController]
[Route("api/conversions")]
public sealed class ConversionsController(IUnitConversionService conversionService) : ControllerBase
{
    /// <summary>Converts a value between two units of the same category.</summary>
    /// <remarks>
    /// Supported categories and units (names are case-insensitive; common symbols such as <c>m</c>, <c>ft</c>,
    /// <c>kg</c>, <c>°C</c> are accepted too):
    ///
    /// * **Length**: Meter, Kilometer, Centimeter, Millimeter, Mile, Yard, Foot, Inch
    /// * **Temperature**: Celsius, Fahrenheit, Kelvin
    /// * **Weight**: Kilogram, Gram, Milligram, Metric Ton, Pound, Ounce
    ///
    /// Sample request:
    ///
    ///     POST /api/conversions
    ///     {
    ///       "value": 100,
    ///       "fromUnit": "Meter",
    ///       "toUnit": "Foot"
    ///     }
    ///
    /// Results are rounded to 10 significant digits.
    /// </remarks>
    /// <param name="request">The value and the units to convert between.</param>
    /// <response code="200">The conversion succeeded.</response>
    /// <response code="400">
    /// The request is invalid: a field is missing or malformed, a unit is not supported, the units belong to
    /// different categories, or the value is not physically valid (e.g. below absolute zero).
    /// </response>
    [HttpPost]
    [ProducesResponseType<ConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public ActionResult<ConversionResponse> Convert([FromBody] ConversionRequest request)
    {
        // [ApiController] has already rejected missing/invalid fields with a 400, so Value is non-null here.
        var response = conversionService.Convert(request.Value!.Value, request.FromUnit, request.ToUnit);
        return Ok(response);
    }
}
