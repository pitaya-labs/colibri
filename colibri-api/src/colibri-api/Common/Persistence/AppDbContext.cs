using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace colibri_api.Common.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : DbContext(options)
{

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

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqliteException inner &&
        inner.SqliteErrorCode == 19 &&
        inner.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);

}
