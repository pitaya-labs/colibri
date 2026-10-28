using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using colibri_api.Common.Persistence;

namespace colibri_api.Tests.Common;

public static class TestDb
{
    public static AppDbContext Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new OwnedConnectionDbContext(options, connection);
        db.Database.EnsureCreated();
        return db;
    }

    public static AppDbContext CreateOn(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        return new AppDbContext(options, NullLogger<AppDbContext>.Instance);
    }

    private sealed class OwnedConnectionDbContext(
        DbContextOptions<AppDbContext> options, SqliteConnection connection)
        : AppDbContext(options, NullLogger<AppDbContext>.Instance)
    {
        public override void Dispose()
        {
            base.Dispose();
            connection.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
