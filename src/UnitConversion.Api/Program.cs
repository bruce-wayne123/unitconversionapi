using System.Text.Json.Serialization;
using UnitConversion.Api.Exceptions;
using UnitConversion.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        // Required fields are declared explicitly with [Required]; this avoids a redundant
        // "The request field is required." entry when the JSON body cannot be parsed.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    // Serialize enums (the response "category") as names, e.g. "Length", instead of numbers.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddUnitConversion();
builder.Services.AddApiDocumentation();

var app = builder.Build();

// Centralized error handling: turns exceptions into ProblemDetails without leaking internals.
app.UseExceptionHandler();
// Gives 404/405/415 and other empty error responses a ProblemDetails body too.
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseApiDocumentation();
app.MapControllers();

app.Run();
