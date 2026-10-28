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
            // Migrations are generated for PostgreSQL; tests run on SQLite and create their own schema.
            if (database.IsNpgsql() && database.GetPendingMigrations().Any())
            {
                database.Migrate();
            }
        }

        return app;
    }
}
