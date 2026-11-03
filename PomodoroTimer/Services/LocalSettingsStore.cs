using PomodoroTimer.Core;
using Windows.Storage;

namespace PomodoroTimer.Services;

/// <summary>Stores settings in the packaged app's ApplicationData.LocalSettings container.</summary>
public sealed class LocalSettingsStore : ISettingsStore
{
    private const string SettingsKey = "AppSettings";

    public AppSettings Load() =>
        AppSettingsSerializer.Deserialize(ApplicationData.Current.LocalSettings.Values[SettingsKey] as string);

    public void Save(AppSettings settings) =>
        ApplicationData.Current.LocalSettings.Values[SettingsKey] = AppSettingsSerializer.Serialize(settings);

    /// <summary>Uses LocalSettings when the app has package identity, otherwise a JSON file under %LocalAppData%.</summary>
    public static ISettingsStore CreateForCurrentProcess()
    {
        try
        {
            _ = Windows.ApplicationModel.Package.Current;
            return new LocalSettingsStore();
        }
        catch (InvalidOperationException)
        {
            return new FileSettingsStore(FileSettingsStore.DefaultFilePath);
        }
    }
}
