namespace PomodoroTimer.Core;

/// <summary>Stores settings as JSON in a file. Used when the app runs without package identity.</summary>
public sealed class FileSettingsStore(string filePath) : ISettingsStore
{
    public static string DefaultFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PomodoroTimer",
        "settings.json");

    public AppSettings Load()
    {
        try
        {
            return File.Exists(filePath)
                ? AppSettingsSerializer.Deserialize(File.ReadAllText(filePath))
                : AppSettings.Default;
        }
        catch (IOException)
        {
            return AppSettings.Default;
        }
        catch (UnauthorizedAccessException)
        {
            return AppSettings.Default;
        }
    }

    public void Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, AppSettingsSerializer.Serialize(settings));
    }
}
