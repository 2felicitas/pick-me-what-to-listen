namespace PickMeWhatToListen.Infrastructure;

/// <summary>Resolves the SQLite catalog file path under the user's local app data folder.</summary>
public static class AppDataDatabasePathProvider
{
    private const string AppFolderName = "PickMeWhatToListen";
    private const string DatabaseFileName = "catalog.db";

    public static string GetDatabaseFilePath()
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appDataFolder, AppFolderName);
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, DatabaseFileName);
    }
}
