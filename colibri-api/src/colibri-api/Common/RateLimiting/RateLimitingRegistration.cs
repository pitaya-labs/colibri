using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace colibri_api.Common.RateLimiting;

public static class RateLimitingRegistration
{
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var options = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue;

                var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown-user"
                    : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    QueueLimit = options.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                });
            });

            limiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                var windowSeconds = context.HttpContext.RequestServices
                    .GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue.WindowSeconds;
                context.HttpContext.Response.Headers.RetryAfter = windowSeconds.ToString();

                await context.HttpContext.RequestServices
                    .GetRequiredService<IProblemDetailsService>()
                    .TryWriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context.HttpContext,
                        ProblemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status429TooManyRequests,
                            Title = "Too many requests.",
                            Type = "https://tools.ietf.org/html/rfc6585#section-4",
                        },
                    });
            };
        });

        return services;
    }

    public static WebApplication UseAppRateLimiting(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}
