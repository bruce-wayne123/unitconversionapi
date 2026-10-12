using Swashbuckle.AspNetCore.SwaggerGen;

namespace UnitConversion.Api.Extensions;

/// <summary>Swagger / OpenAPI setup, kept out of Program.cs.</summary>
public static class ApiDocumentationExtensions
{
    /// <summary>Registers the OpenAPI generator and includes this assembly's XML doc comments.</summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{typeof(ApiDocumentationExtensions).Assembly.GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
        });
        return services;
    }

    /// <summary>
    /// Serves the OpenAPI document and Swagger UI at <c>/swagger</c>, and redirects <c>/</c> there so the API
    /// can be tried straight from the browser.
    /// </summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        return app;
    }
}
