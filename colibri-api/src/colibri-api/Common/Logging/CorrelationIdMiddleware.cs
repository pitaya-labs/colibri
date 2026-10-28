using Serilog.Context;

namespace colibri_api.Common.Logging;

public static class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";

    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers[HeaderName] = context.TraceIdentifier;

            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
                await next();
        });

        return app;
    }
}
