using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

namespace colibri_api.Common.OpenApi;

public static class OpenApiRegistration
{
    public static IServiceCollection AddOpenApiDocument(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            options.CreateSchemaReferenceId = typeInfo =>
                typeInfo.Type.DeclaringType is { } owner
                    ? $"{owner.Name}{typeInfo.Type.Name}"
                    : OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
        });

    public static WebApplication MapOpenApiDocument(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}
