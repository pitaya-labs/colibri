using Microsoft.EntityFrameworkCore;

namespace colibri_api.Common.Persistence;

public static class AppDbContextRegistration
{
    public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default") ?? "Data Source=colibri_api.db"));

    public static WebApplication ApplyPendingMigrations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var pendingMigrations = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            }
        }

        return app;
    }
}
