namespace PomodoroTimer.Core;

public interface ISettingsStore
{
    AppSettings Load();

    void Save(AppSettings settings);
}
