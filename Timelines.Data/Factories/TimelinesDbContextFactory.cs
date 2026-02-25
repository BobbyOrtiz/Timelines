using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Timelines.Data.Factories;

/// <summary>
/// Design-time factory for creating TimelinesDbContext during migrations.
/// Reads connection string from environment variable TIMELINES_CONNECTION_STRING.
/// </summary>
public class TimelinesDbContextFactory : IDesignTimeDbContextFactory<TimelinesDbContext>
{
    public TimelinesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TIMELINES_CONNECTION_STRING")
            ?? "Server=localhost;Database=Timelines.Dev;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<TimelinesDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TimelinesDbContext(optionsBuilder.Options);
    }
}
