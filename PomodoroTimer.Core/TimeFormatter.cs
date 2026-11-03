using System.Globalization;

namespace PomodoroTimer.Core;

public static class TimeFormatter
{
    /// <summary>Formats as mm:ss, rounding partial seconds up so the display never shows 00:00 early.</summary>
    public static string Format(TimeSpan remaining)
    {
        var totalSeconds = remaining <= TimeSpan.Zero ? 0 : (long)Math.Ceiling(remaining.TotalSeconds);
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
    }
}
