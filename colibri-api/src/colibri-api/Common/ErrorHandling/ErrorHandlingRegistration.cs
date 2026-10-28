namespace colibri_api.Common.ErrorHandling;

public static class ErrorHandlingRegistration
{
    public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services, IHostEnvironment environment) =>
        services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    if (environment.IsDevelopment() && context.Exception is not null)
                        context.ProblemDetails.Extensions["exception"] = context.Exception.ToString();
                };
            });

    public static WebApplication UseGlobalErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
