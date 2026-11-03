namespace PomodoroTimer.Tests;

public class PomodoroEngineTests
{
    private static readonly TimeSpan Work = TimeSpan.FromMinutes(25);
    private static readonly TimeSpan Break = TimeSpan.FromMinutes(5);

    private static PomodoroEngine CreateEngine() => new(Work, Break);

    [Fact]
    public void NewEngine_IsIdleAtStartOfWork()
    {
        var engine = CreateEngine();

        Assert.Equal(TimerPhase.Work, engine.Phase);
        Assert.Equal(Work, engine.Remaining);
        Assert.False(engine.IsRunning);
    }

    [Fact]
    public void Tick_WhenNotRunning_DoesNotChangeRemaining()
    {
        var engine = CreateEngine();

        engine.Tick(TimeSpan.FromSeconds(10));

        Assert.Equal(Work, engine.Remaining);
    }

    [Fact]
    public void Start_ThenTick_DecreasesRemaining()
    {
        var engine = CreateEngine();

        engine.Start();
        engine.Tick(TimeSpan.FromSeconds(1));

        Assert.True(engine.IsRunning);
        Assert.Equal(Work - TimeSpan.FromSeconds(1), engine.Remaining);
    }

    [Fact]
    public void Pause_StopsCountdown()
    {
        var engine = CreateEngine();
        engine.Start();
        engine.Tick(TimeSpan.FromSeconds(3));

        engine.Pause();
        engine.Tick(TimeSpan.FromSeconds(10));

        Assert.False(engine.IsRunning);
        Assert.Equal(Work - TimeSpan.FromSeconds(3), engine.Remaining);
    }

    [Fact]
    public void Start_AfterPause_Resumes()
    {
        var engine = CreateEngine();
        engine.Start();
        engine.Tick(TimeSpan.FromSeconds(3));
        engine.Pause();

        engine.Start();
        engine.Tick(TimeSpan.FromSeconds(2));

        Assert.Equal(Work - TimeSpan.FromSeconds(5), engine.Remaining);
    }

    [Fact]
    public void Reset_StopsAndReturnsToStartOfWork()
    {
        var engine = CreateEngine();
        engine.Start();
        engine.Tick(Work + TimeSpan.FromSeconds(30)); // now in Break

        engine.Reset();

        Assert.False(engine.IsRunning);
        Assert.Equal(TimerPhase.Work, engine.Phase);
        Assert.Equal(Work, engine.Remaining);
    }

    [Fact]
    public void ReachingZero_SwitchesToBreak_AndKeepsRunning()
    {
        var engine = CreateEngine();
        engine.Start();

        engine.Tick(Work);

        Assert.Equal(TimerPhase.Break, engine.Phase);
        Assert.Equal(Break, engine.Remaining);
        Assert.True(engine.IsRunning);
    }

    [Fact]
    public void BreakEnding_SwitchesBackToWork()
    {
        var engine = CreateEngine();
        engine.Start();

        engine.Tick(Work);
        engine.Tick(Break);

        Assert.Equal(TimerPhase.Work, engine.Phase);
        Assert.Equal(Work, engine.Remaining);
        Assert.True(engine.IsRunning);
    }

    [Fact]
    public void Overshoot_CarriesIntoNextPhase()
    {
        var engine = CreateEngine();
        engine.Start();

        engine.Tick(Work + TimeSpan.FromSeconds(2));

        Assert.Equal(TimerPhase.Break, engine.Phase);
        Assert.Equal(Break - TimeSpan.FromSeconds(2), engine.Remaining);
    }

    [Fact]
    public void PhaseChanged_IsRaisedOnSwitch()
    {
        var engine = CreateEngine();
        var phases = new List<TimerPhase>();
        engine.PhaseChanged += (_, phase) => phases.Add(phase);
        engine.Start();

        engine.Tick(Work);
        engine.Tick(Break);

        Assert.Equal([TimerPhase.Break, TimerPhase.Work], phases);
    }

    [Fact]
    public void StateChanged_IsRaisedOnStartTickPauseReset()
    {
        var engine = CreateEngine();
        var count = 0;
        engine.StateChanged += (_, _) => count++;

        engine.Start();
        engine.Tick(TimeSpan.FromSeconds(1));
        engine.Pause();
        engine.Reset();

        Assert.Equal(4, count);
    }

    [Fact]
    public void UpdateDurations_WhileRunning_DoesNotChangeCurrentInterval()
    {
        var engine = CreateEngine();
        engine.Start();
        engine.Tick(TimeSpan.FromMinutes(1));

        engine.UpdateDurations(TimeSpan.FromMinutes(50), TimeSpan.FromMinutes(10));

        Assert.Equal(TimeSpan.FromMinutes(24), engine.Remaining);
    }

    [Fact]
    public void UpdateDurations_WhilePausedMidInterval_DoesNotChangeCurrentInterval()
    {
        var engine = CreateEngine();
        engine.Start();
        engine.Tick(TimeSpan.FromMinutes(1));
        engine.Pause();

        engine.UpdateDurations(TimeSpan.FromMinutes(50), TimeSpan.FromMinutes(10));

        Assert.Equal(TimeSpan.FromMinutes(24), engine.Remaining);
    }

    [Fact]
    public void UpdateDurations_AppliesToNextPhase()
    {
        var engine = CreateEngine();
        engine.Start();
        engine.Tick(TimeSpan.FromMinutes(1));

        engine.UpdateDurations(TimeSpan.FromMinutes(50), TimeSpan.FromMinutes(10));
        engine.Tick(TimeSpan.FromMinutes(24));

        Assert.Equal(TimerPhase.Break, engine.Phase);
        Assert.Equal(TimeSpan.FromMinutes(10), engine.Remaining);
    }

    [Fact]
    public void UpdateDurations_WhenIdleAtPhaseStart_UpdatesRemaining()
    {
        var engine = CreateEngine();

        engine.UpdateDurations(TimeSpan.FromMinutes(50), TimeSpan.FromMinutes(10));

        Assert.Equal(TimeSpan.FromMinutes(50), engine.Remaining);
    }

    [Fact]
    public void Reset_UsesLatestDurations()
    {
        var engine = CreateEngine();
        engine.Start();
        engine.Tick(TimeSpan.FromMinutes(1));
        engine.UpdateDurations(TimeSpan.FromMinutes(40), TimeSpan.FromMinutes(10));

        engine.Reset();

        Assert.Equal(TimeSpan.FromMinutes(40), engine.Remaining);
    }

    [Fact]
    public void Constructor_RejectsNonPositiveDuration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PomodoroEngine(TimeSpan.Zero, Break));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PomodoroEngine(Work, TimeSpan.FromSeconds(-1)));
    }
}
