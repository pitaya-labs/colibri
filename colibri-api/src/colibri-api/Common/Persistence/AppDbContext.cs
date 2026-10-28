using colibri_api.Features.Assignments.Domain;
using colibri_api.Features.Clinics.Domain;
using colibri_api.Features.Doctors.Domain;
using colibri_api.Features.Tenants.Domain;
using colibri_api.Features.Users.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace colibri_api.Common.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MfaCredential> MfaCredentials => Set<MfaCredential>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    public async Task<SaveResult> TrySaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<IHasRowVersion>())
            if (entry.State == EntityState.Modified)
                entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();

        try
        {
            await SaveChangesAsync(ct);
            return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict while saving changes.");
            return SaveResult.ConcurrencyConflict;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Unique constraint violation while saving changes.");
            return SaveResult.DuplicateKey;
        }
    }

    // PostgreSQL in production (SqlState 23505); in-memory SQLite in the tests.
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException switch
        {
            PostgresException pg => pg.SqlState == PostgresErrorCodes.UniqueViolation,
            SqliteException lite => lite.SqliteErrorCode == 19 &&
                lite.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase),
            _ => false
        };

}
