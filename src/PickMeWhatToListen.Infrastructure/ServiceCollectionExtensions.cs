using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickMeWhatToListen.Application;
using PickMeWhatToListen.Application.Abstractions;
using PickMeWhatToListen.Infrastructure.CoverArtArchive;
using PickMeWhatToListen.Infrastructure.MusicBrainz;

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
        services.AddSingleton<IArtistMetadataRepository, EfArtistMetadataRepository>();
        services.AddSingleton<IRandomProvider, SystemRandomProvider>();
        services.AddSingleton<MusicBrainzRateLimiter>();
        services.AddSingleton<CoverArtArchiveRateLimiter>();

        services.AddHttpClient<IArtistMetadataProvider, MusicBrainzMetadataProvider>(client =>
        {
            // Requests use absolute URIs built in MusicBrainzMetadataProvider — no BaseAddress.
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(HttpClientUserAgent.Value);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        services.AddHttpClient<ICoverArtProvider, CoverArtArchiveProvider>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(HttpClientUserAgent.Value);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        services.AddSingleton<CoverArtEnrichmentCoordinator>();
        services.AddSingleton<ICoverArtEnrichmentQueue>(sp => sp.GetRequiredService<CoverArtEnrichmentCoordinator>());
        services.AddSingleton<ICoverArtEnrichmentNotifier>(sp => sp.GetRequiredService<CoverArtEnrichmentCoordinator>());
        services.AddHostedService<CoverArtEnrichmentWorker>();
        services.AddScoped<CoverArtEnrichmentService>();

        services.AddTransient<ArtistProfileService>();

        return services;
    }
}
