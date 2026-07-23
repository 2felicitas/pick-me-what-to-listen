using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PickMeWhatToListen.Infrastructure;

/// <summary>
/// Lets `dotnet ef migrations` construct <see cref="AppDbContext"/> at design time
/// without needing to boot the WPF host.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var databasePath = AppDataDatabasePathProvider.GetDatabaseFilePath();
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}");

        return new AppDbContext(optionsBuilder.Options);
    }
}
