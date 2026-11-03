using System.Text.Json;
using System.Text.Json.Serialization;

namespace PomodoroTimer.Core;

public static class AppSettingsSerializer
{
    public static string Serialize(AppSettings settings) =>
        JsonSerializer.Serialize(SettingsDto.From(settings.Normalize()), SettingsJsonContext.Default.SettingsDto);

    /// <summary>Parses stored settings; any missing, invalid, or unreadable data falls back to defaults.</summary>
    public static AppSettings Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return AppSettings.Default;
        }

        try
        {
            var dto = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.SettingsDto);
            return dto?.ToSettings().Normalize() ?? AppSettings.Default;
        }
        catch (JsonException)
        {
            return AppSettings.Default;
        }
    }

    internal sealed class SettingsDto
    {
        public int WorkMinutes { get; set; } = AppSettings.Default.WorkMinutes;

        public int BreakMinutes { get; set; } = AppSettings.Default.BreakMinutes;

        public AppTheme Theme { get; set; } = AppSettings.Default.Theme;

        public static SettingsDto From(AppSettings settings) => new()
        {
            WorkMinutes = settings.WorkMinutes,
            BreakMinutes = settings.BreakMinutes,
            Theme = settings.Theme,
        };

        public AppSettings ToSettings() => new(WorkMinutes, BreakMinutes, Theme);
    }
}

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(AppSettingsSerializer.SettingsDto))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;
