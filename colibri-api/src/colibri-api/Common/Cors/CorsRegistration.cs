namespace colibri_api.Common.Cors;

public static class CorsRegistration
{
    private const string ConfigSection = "Cors";

    public static IServiceCollection AddAppCors(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration) =>
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    return;
                }

                var allowedOrigins = configuration.GetSection($"{ConfigSection}:AllowedOrigins").Get<string[]>() ?? [];
                policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
            }));
}
