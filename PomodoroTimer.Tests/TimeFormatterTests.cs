namespace PomodoroTimer.Tests;

public class TimeFormatterTests
{
    [Theory]
    [InlineData(25 * 60, "25:00")]
    [InlineData(5 * 60, "05:00")]
    [InlineData(59, "00:59")]
    [InlineData(0, "00:00")]
    [InlineData(120 * 60, "120:00")]
    public void Format_WholeSeconds(int seconds, string expected)
    {
        Assert.Equal(expected, TimeFormatter.Format(TimeSpan.FromSeconds(seconds)));
    }

    [Fact]
    public void Format_RoundsPartialSecondsUp()
    {
        // 24:59.4 remaining still shows 25:00 until a full second has elapsed.
        Assert.Equal("25:00", TimeFormatter.Format(TimeSpan.FromSeconds(25 * 60 - 0.6)));
    }

    [Fact]
    public void Format_NegativeIsZero()
    {
        Assert.Equal("00:00", TimeFormatter.Format(TimeSpan.FromSeconds(-3)));
    }
}
