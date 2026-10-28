using Microsoft.EntityFrameworkCore;

namespace colibri_api.Common.Persistence;

public static class AppDbContextRegistration
{
    public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

    public static WebApplication ApplyPendingMigrations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
            // Las migraciones se generan para PostgreSQL; los tests corren sobre SQLite y crean su esquema aparte.
            if (database.IsNpgsql() && database.GetPendingMigrations().Any())
            {
                database.Migrate();
            }
        }

        return app;
    }
}
