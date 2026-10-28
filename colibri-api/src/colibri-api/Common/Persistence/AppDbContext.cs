using colibri_api.Features.Adscripciones.Domain;
using colibri_api.Features.Consultorios.Domain;
using colibri_api.Features.Medicos.Domain;
using colibri_api.Features.Tenants.Domain;
using colibri_api.Features.Usuarios.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace colibri_api.Common.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<MfaCredencial> MfaCredenciales => Set<MfaCredencial>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<Consultorio> Consultorios => Set<Consultorio>();
    public DbSet<Adscripcion> Adscripciones => Set<Adscripcion>();

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

    // PostgreSQL en producción (SqlState 23505); SQLite en memoria en los tests.
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException switch
        {
            PostgresException pg => pg.SqlState == PostgresErrorCodes.UniqueViolation,
            SqliteException lite => lite.SqliteErrorCode == 19 &&
                lite.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase),
            _ => false
        };

}
