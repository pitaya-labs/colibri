using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using colibri_api.Common.Persistence;

namespace colibri_api.Common.HealthChecks;

public static class HealthCheckRegistration
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services) =>
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(tags: [ReadyTag])
            .Services;

    public static WebApplication MapAppHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
            .ExcludeFromDescription()
            .DisableRateLimiting();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) })
            .ExcludeFromDescription()
            .DisableRateLimiting();

        return app;
    }
}
