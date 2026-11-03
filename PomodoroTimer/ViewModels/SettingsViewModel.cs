using CommunityToolkit.Mvvm.ComponentModel;
using PomodoroTimer.Core;

namespace PomodoroTimer.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _store;
    private readonly PomodoroEngine _engine;
    private AppSettings _settings;

    public SettingsViewModel(ISettingsStore store, PomodoroEngine engine, AppSettings initialSettings)
    {
        _store = store;
        _engine = engine;
        _settings = initialSettings;
        WorkMinutes = initialSettings.WorkMinutes;
        BreakMinutes = initialSettings.BreakMinutes;
        ThemeIndex = (int)initialSettings.Theme;
    }

    /// <summary>Raised when the user picks a different theme.</summary>
    public event EventHandler<AppTheme>? ThemeChanged;

    public double MinMinutes => AppSettings.MinMinutes;

    public double MaxWorkMinutes => AppSettings.MaxWorkMinutes;

    public double MaxBreakMinutes => AppSettings.MaxBreakMinutes;

    public AppTheme Theme => _settings.Theme;

    // NumberBox.Value is a double and becomes NaN when the box is cleared.
    [ObservableProperty]
    public partial double WorkMinutes { get; set; }

    [ObservableProperty]
    public partial double BreakMinutes { get; set; }

    /// <summary>Index into the Light/Dark radio buttons; matches <see cref="AppTheme"/> values.</summary>
    [ObservableProperty]
    public partial int ThemeIndex { get; set; }

    partial void OnWorkMinutesChanged(double value)
    {
        if (double.IsNaN(value))
        {
            WorkMinutes = _settings.WorkMinutes;
            return;
        }

        Apply(_settings with { WorkMinutes = (int)Math.Round(value) });
        WorkMinutes = _settings.WorkMinutes; // reflect rounding/clamping back to the NumberBox
    }

    partial void OnBreakMinutesChanged(double value)
    {
        if (double.IsNaN(value))
        {
            BreakMinutes = _settings.BreakMinutes;
            return;
        }

        Apply(_settings with { BreakMinutes = (int)Math.Round(value) });
        BreakMinutes = _settings.BreakMinutes; // reflect rounding/clamping back to the NumberBox
    }

    partial void OnThemeIndexChanged(int value)
    {
        if (!Enum.IsDefined((AppTheme)value))
        {
            return;
        }

        var theme = (AppTheme)value;
        if (theme == _settings.Theme)
        {
            return;
        }

        Apply(_settings with { Theme = theme });
        ThemeChanged?.Invoke(this, theme);
    }

    private void Apply(AppSettings updated)
    {
        updated = updated.Normalize();
        if (updated == _settings)
        {
            return;
        }

        _settings = updated;
        _store.Save(updated);
        _engine.UpdateDurations(
            TimeSpan.FromMinutes(updated.WorkMinutes),
            TimeSpan.FromMinutes(updated.BreakMinutes));
    }
}
