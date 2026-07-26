namespace PickMeWhatToListen.Application;

/// <summary>Tally produced by <see cref="ArtistCatalogService.AddArtistsAsync"/>.</summary>
public sealed record BulkAddArtistsResult(int AddedCount, int SkippedDuplicateCount, int SkippedInvalidCount);
