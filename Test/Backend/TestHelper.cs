using Backend.Data;
using Dorssel.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Test;

public class TestHelper
{
    public static GridDbContext CreateTestDbContext(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open(); // Keep open during the test
        var options = new DbContextOptionsBuilder<GridDbContext>()
            .UseSqlite(connection)
            .UseSqliteTimestamp()
            .Options;
        var dbContext = new GridDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    public static GridDbContext Setup(string connectionString)
    {
        GridDbContext dbContext = CreateTestDbContext(connectionString);
        DataGenerator.SeedData(dbContext, 10000);
        dbContext.SaveChanges();
        return dbContext;
    }
}