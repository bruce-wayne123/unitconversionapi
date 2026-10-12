# Unit Conversion API

A small, layered ASP.NET Core Web API that converts numeric values between units of **length**,
**temperature** and **weight/mass**. Built for clarity and extensibility: all unit knowledge lives in a
data-driven registry, so adding a unit is one line and adding hundreds is the same job repeated.

## Supported conversions

| Category    | Units (canonical names)                                       | Base unit |
|-------------|---------------------------------------------------------------|-----------|
| Length      | Meter, Kilometer, Centimeter, Millimeter, Mile, Yard, Foot, Inch | Meter     |
| Temperature | Celsius, Fahrenheit, Kelvin                                   | Kelvin    |
| Weight      | Kilogram, Gram, Milligram, Metric Ton, Pound, Ounce           | Kilogram  |

* Any unit converts to any other unit **in the same category**.
* Unit names are **case-insensitive** and surrounding whitespace is ignored (`"  fOOt "` works).
* Common symbols/aliases are accepted too: `m`, `km`, `cm`, `mm`, `mi`, `yd`, `ft`, `in`, `C`/`°C`, `F`/`°F`, `K`,
  `kg`, `g`, `mg`, `t`, `tonne`, `MetricTon`, `lb`/`lbs`, `oz`, `metre`, ...
* Unknown units are **never** silently accepted.
* Decimal values are supported. Negative values are accepted where physically meaningful (see
  [Design decisions](#design-decisions-and-assumptions)).

## Prerequisites

* [.NET SDK 10.0](https://dotnet.microsoft.com/download) (the current LTS release). Check with `dotnet --version`.
  If you only have an older SDK, change `<TargetFramework>` in `Directory.Build.props` (e.g. to `net8.0`);
  the code only uses APIs available since .NET 8.
* Internet access on first build to restore NuGet packages (Swashbuckle, xUnit, test SDK).

## Build and run

```bash
dotnet restore
dotnet build

dotnet run --project src/UnitConversion.Api
```

The API listens on **http://localhost:5080** (see `src/UnitConversion.Api/Properties/launchSettings.json`).
Open **http://localhost:5080/swagger** (or just `/`, which redirects there) to try the API from Swagger UI:
expand `POST /api/conversions` → **Try it out** → **Execute**.

From a terminal:

```bash
curl -X POST http://localhost:5080/api/conversions \
  -H "Content-Type: application/json" \
  -d '{"value": 100, "fromUnit": "Meter", "toUnit": "Foot"}'
```

## Run the unit tests

```bash
dotnet test
```

The tests (xUnit) cover the required scenarios and more: meter→foot, kilometer→meter, Celsius↔Fahrenheit,
Celsius→Kelvin, kilogram→pound, gram→kilogram, case-insensitive names, unsupported units, incompatible
categories, invalid values, absolute-zero boundaries, round trips, registry validation, rounding, DTO validation,
the controller and the global exception handler.

## API reference

### `POST /api/conversions`

Request body (`application/json`):

| Field      | Type   | Required | Description                                                      |
|------------|--------|----------|------------------------------------------------------------------|
| `value`    | number | yes      | Value to convert. Decimals allowed.                              |
| `fromUnit` | string | yes      | Unit of `value` (case-insensitive, max 50 characters).           |
| `toUnit`   | string | yes      | Target unit, same category as `fromUnit` (case-insensitive).     |

Responses:

| Status | When                                                                                                  |
|--------|-------------------------------------------------------------------------------------------------------|
| 200 OK | The conversion succeeded.                                                                             |
| 400 Bad Request | Missing/malformed fields, unsupported unit, incompatible categories, or an invalid value.      |
| 500    | Unexpected server error (generic message only; details are logged, never returned).                   |

### Sample requests and responses

**Length**

```json
POST /api/conversions
{ "value": 100, "fromUnit": "Meter", "toUnit": "Foot" }
```
```json
200 OK
{
  "originalValue": 100,
  "fromUnit": "Meter",
  "convertedValue": 328.0839895,
  "toUnit": "Foot",
  "category": "Length"
}
```

**Temperature, case-insensitive input** — the response always uses canonical unit names:

```json
POST /api/conversions
{ "value": 100, "fromUnit": "cELSIUS", "toUnit": "f" }
```
```json
200 OK
{
  "originalValue": 100,
  "fromUnit": "Celsius",
  "convertedValue": 212,
  "toUnit": "Fahrenheit",
  "category": "Temperature"
}
```

**Weight**

```json
POST /api/conversions
{ "value": 1, "fromUnit": "Kilogram", "toUnit": "Pound" }
```
```json
200 OK
{
  "originalValue": 1,
  "fromUnit": "Kilogram",
  "convertedValue": 2.204622622,
  "toUnit": "Pound",
  "category": "Weight"
}
```

## Error-handling behavior

Errors use the standard **RFC 9457 problem details** format (`application/problem+json`).

**Business-rule errors** (unsupported unit, incompatible categories, invalid value) are thrown as typed exceptions by
the service and turned into `400` by one central handler. They add a stable machine-readable `code`:

| `code`                    | Meaning                                                              |
|---------------------------|----------------------------------------------------------------------|
| `unsupported_unit`        | `fromUnit` / `toUnit` is not a registered unit.                      |
| `incompatible_categories` | The units belong to different categories (e.g. Meter → Celsius).     |
| `invalid_value`           | Below absolute zero, negative weight, or too large to represent.     |

```json
POST /api/conversions
{ "value": 1, "fromUnit": "Meter", "toUnit": "Celsius" }
```
```json
400 Bad Request
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "The conversion request is invalid.",
  "status": 400,
  "detail": "Cannot convert Meter (Length) to Celsius (Temperature): units must belong to the same category.",
  "code": "incompatible_categories",
  "traceId": "00-..."
}
```

```json
POST /api/conversions
{ "value": -300, "fromUnit": "Celsius", "toUnit": "Kelvin" }
```
```json
400 Bad Request
{
  "status": 400,
  "title": "The conversion request is invalid.",
  "detail": "-300 Celsius is not a valid Temperature value: it is below absolute zero (0 K).",
  "code": "invalid_value"
}
```

**Request-shape errors** (missing fields, blank units, wrong JSON types, malformed JSON) are rejected before the
service runs by `[ApiController]` model validation, which returns a `400` validation problem with an `errors` map:

```json
POST /api/conversions
{}
```
```json
400 Bad Request
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Value":    ["The 'value' field is required."],
    "ToUnit":   ["The 'toUnit' field is required."],
    "FromUnit": ["The 'fromUnit' field is required."]
  }
}
```

**Unexpected errors** return `500` with a generic message. The exception, message and stack trace are logged
server-side only and are never exposed to the caller. Unknown routes and methods (404/405) also get a problem-details body.

## Project structure

```
UnitConversion.sln
Directory.Build.props                  Shared settings: net10.0, nullable, implicit usings
README.md
.gitignore
src/
  UnitConversion.Api/
    Program.cs                         Composition root and middleware pipeline
    Controllers/
      ConversionsController.cs         POST /api/conversions (HTTP concerns only)
    Models/
      ConversionRequest.cs             Request DTO + validation attributes
      ConversionResponse.cs            Response DTO
    Services/
      IUnitConversionService.cs        Abstraction used by the controller
      UnitConversionService.cs         Conversion pipeline: source → base → target
      DecimalRounding.cs               Significant-digit rounding helper
    Units/
      UnitCategory.cs                  Length / Temperature / Weight
      UnitDefinition.cs                One unit: name, aliases, to/from base functions
      CategoryDefinition.cs            Per-category base unit and physical minimum
      IUnitRegistry.cs / UnitRegistry.cs   Case-insensitive lookup + catalog validation
      UnitCatalog.cs                   The data: all units and named constants
    Exceptions/
      UnitConversionException.cs       Base type for caller errors (→ 400)
      UnsupportedUnitException.cs
      IncompatibleUnitCategoriesException.cs
      InvalidConversionValueException.cs
      GlobalExceptionHandler.cs        Exceptions → ProblemDetails (no internal details)
    Extensions/
      ServiceCollectionExtensions.cs   DI registration
      ApiDocumentationExtensions.cs    Swagger / OpenAPI setup
    Properties/launchSettings.json
    appsettings*.json
tests/
  UnitConversion.UnitTests/            xUnit tests
```

## Design decisions and assumptions

* **Layering.** The controller only maps HTTP to a service call. `IUnitConversionService` /
  `UnitConversionService` hold the rules and know nothing about HTTP. Unit data lives in the registry.
  This stays at three small layers deliberately; no repositories, mediators or mappers.
* **Hub-and-spoke conversion.** Every conversion is `source → base unit → target`. There are no per-pair branches;
  *N* units need *N* definitions instead of *N × N* rules.
* **Temperature uses formulas, not factors.** Celsius and Fahrenheit have offsets, so each unit supplies explicit
  to-Kelvin / from-Kelvin formulas in `UnitCatalog`. The Fahrenheit formulas multiply before dividing
  (`(F + 459.67) × 5 / 9`) to avoid precision loss.
* **`decimal` everywhere.** Values and factors are `decimal`, so `0.1`-style inputs and exact definitions
  (1 ft = 0.3048 m, 1 lb = 0.45359237 kg) are represented exactly and 100 °C converts to exactly `212`, not `211.99999999999997`.
  The trade-off is a smaller range (~±7.9×10²⁸); out-of-range input returns a clean `400`, not a 500.
* **Rounding.** Results are rounded to **10 significant digits** (half away from zero) with trailing zeros removed.
  Significant digits, not decimal places, so very small results such as 1 mg = 0.000000001 t are not rounded to zero. This
  also reproduces the example in the brief (`328.0839895`).
* **Negative values and physical limits.** Each category can declare a minimum (in its base unit).
  * Length: no minimum. Negative values are valid as signed displacement.
  * Temperature: minimum 0 K, so anything below absolute zero is rejected (the boundary itself is valid).
  * Weight: minimum 0 kg, so negative weights are rejected.

  If you disagree with a rule, change one `CategoryDefinition` in `UnitCatalog`.
* **"Weight/Mass".** The category is named `Weight` in the API, following the brief's heading; the units are masses.
* **Canonical names in responses.** `fromUnit`/`toUnit` in the response are the canonical names, so clients see a
  normalized value even if they sent `"ft"` or `"METER"`.
* **Synchronous service.** The work is pure in-memory arithmetic, so async would add overhead and no benefit.
* **Fail fast.** The registry validates the catalog at start-up (duplicate names/aliases, missing base unit,
  missing category definition), so a bad catalog entry crashes at boot rather than producing wrong answers.
* **Singleton services.** Registry and service are immutable and stateless, so they are registered as singletons.
* **Error handling.** One `IExceptionHandler` maps domain exceptions to `400` and everything else to a generic `500`.
  Domain exception messages are written for consumers; no other exception text is ever returned.
* **Dependencies.** Only `Swashbuckle.AspNetCore` in the API (Swagger UI) plus the standard xUnit packages for tests.
* **HTTPS.** HTTPS redirection is enabled outside Development; local runs use plain HTTP so Swagger works out of the box.
* **Assumption: unit names are matched case-insensitively but otherwise literally.** `"Metric Ton"`, `"metric ton"`,
  `"MetricTon"` and `"tonne"` all work, but `"metric  ton"` (two spaces) does not.

## How the architecture scales to hundreds of units

1. **Adding a unit is data, not logic.** Proportional units are one line in `UnitCatalog.CreateUnits()`:

   ```csharp
   UnitDefinition.Linear("Nautical Mile", UnitCategory.Length, 1852m, "nmi"),
   ```

   `UnitConversionService`, the controller and the DTOs do not change. A unit test proves this by adding a custom unit to
   a registry and converting through the unchanged service.
2. **Linear cost.** Because of the base-unit hub, 300 length units need 300 definitions, not 90,000 conversion pairs.
3. **O(1) lookup.** The registry is a case-insensitive dictionary keyed by name and alias, so lookup speed does not
   depend on how many units exist.
4. **Non-proportional units are supported.** A unit that needs a formula (Rankine, Réaumur, fuel economy, ...) passes
   its own `toBase`/`fromBase` functions, as temperature already does.
5. **New categories are additive.** Add a value to `UnitCategory`, a `CategoryDefinition` (base unit, optional physical
   minimum) and the units. Category-mismatch checking and error handling already work generically.
6. **Mistakes are caught early.** Duplicate names/aliases or a missing base unit fail at start-up.
7. **Easy to externalize.** The service depends only on `IUnitRegistry`. The catalog could later be loaded from JSON or a
   database by providing a different registry source, with no change to the conversion logic.

## Known limitations

* No `GET` endpoint to list supported units (they are documented in Swagger and this README).
* Unit matching is case-insensitive via ordinal comparison, so two future units that differ only by case (e.g. `mm` vs `Mm`)
  could not both be registered; the registry reports this as a duplicate at start-up.
* Request-shape validation errors use the framework's `ValidationProblemDetails` format (with an `errors` map), which differs
  slightly from the `code`-based format used for business-rule errors.
* No authentication, rate limiting, or persistence; out of scope for this assessment.
