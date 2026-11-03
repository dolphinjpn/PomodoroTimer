namespace PomodoroTimer.Core;

public enum AppTheme
{
    Light,
    Dark,
}

public sealed record AppSettings(int WorkMinutes, int BreakMinutes, AppTheme Theme)
{
    public const int MinMinutes = 1;
    public const int MaxWorkMinutes = 120;
    public const int MaxBreakMinutes = 60;

    public static AppSettings Default { get; } = new(25, 5, AppTheme.Light);

    /// <summary>Returns a copy with minutes clamped to the supported range and a known theme.</summary>
    public AppSettings Normalize() => new(
        Math.Clamp(WorkMinutes, MinMinutes, MaxWorkMinutes),
        Math.Clamp(BreakMinutes, MinMinutes, MaxBreakMinutes),
        Enum.IsDefined(Theme) ? Theme : Default.Theme);
}
