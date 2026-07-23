using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PickMeWhatToListen.Application.Abstractions;

namespace PickMeWhatToListen.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQLite-backed <see cref="AppDbContext"/> and its repository/adapters.
    /// Kept here so callers (e.g. the WPF composition root) never need to reference EF Core directly.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var databasePath = AppDataDatabasePathProvider.GetDatabaseFilePath();

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddSingleton<IArtistRepository, EfArtistRepository>();
        services.AddSingleton<IRandomProvider, SystemRandomProvider>();

        return services;
    }
}
