namespace PomodoroTimer.Tests;

public class AppSettingsTests
{
    [Fact]
    public void Defaults_Are25And5AndLight()
    {
        var settings = AppSettings.Default;

        Assert.Equal(25, settings.WorkMinutes);
        Assert.Equal(5, settings.BreakMinutes);
        Assert.Equal(AppTheme.Light, settings.Theme);
    }

    [Fact]
    public void SerializeDeserialize_RoundTrips()
    {
        var settings = new AppSettings(40, 10, AppTheme.Dark);

        var json = AppSettingsSerializer.Serialize(settings);
        var restored = AppSettingsSerializer.Deserialize(json);

        Assert.Equal(settings, restored);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[1,2,3]")]
    public void Deserialize_InvalidInput_ReturnsDefaults(string? json)
    {
        Assert.Equal(AppSettings.Default, AppSettingsSerializer.Deserialize(json));
    }

    [Fact]
    public void Deserialize_MissingFields_UsesDefaults()
    {
        var restored = AppSettingsSerializer.Deserialize("""{"Theme":"Dark"}""");

        Assert.Equal(new AppSettings(25, 5, AppTheme.Dark), restored);
    }

    [Fact]
    public void Deserialize_OutOfRangeValues_AreClamped()
    {
        var restored = AppSettingsSerializer.Deserialize("""{"WorkMinutes":0,"BreakMinutes":999,"Theme":"Light"}""");

        Assert.Equal(AppSettings.MinMinutes, restored.WorkMinutes);
        Assert.Equal(AppSettings.MaxBreakMinutes, restored.BreakMinutes);
    }

    [Fact]
    public void Deserialize_UnknownTheme_FallsBackToDefault()
    {
        var restored = AppSettingsSerializer.Deserialize("""{"WorkMinutes":30,"BreakMinutes":5,"Theme":"Purple"}""");

        Assert.Equal(AppSettings.Default, restored);
    }

    [Fact]
    public void Normalize_ClampsWorkMinutes()
    {
        var normalized = new AppSettings(500, 5, AppTheme.Light).Normalize();

        Assert.Equal(AppSettings.MaxWorkMinutes, normalized.WorkMinutes);
    }

    [Fact]
    public void FileSettingsStore_LoadWithoutFile_ReturnsDefaults()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "settings.json");
        var store = new FileSettingsStore(path);

        Assert.Equal(AppSettings.Default, store.Load());
    }

    [Fact]
    public void FileSettingsStore_SaveThenLoad_RoundTrips()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var settings = new AppSettings(45, 15, AppTheme.Dark);

            new FileSettingsStore(path).Save(settings);
            var restored = new FileSettingsStore(path).Load();

            Assert.Equal(settings, restored);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
