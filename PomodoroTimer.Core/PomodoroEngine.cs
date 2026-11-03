namespace PomodoroTimer.Core;

/// <summary>
/// UI-independent Pomodoro countdown. Time only advances through <see cref="Tick"/>,
/// so callers (a UI timer or a unit test) decide how time passes.
/// </summary>
public sealed class PomodoroEngine
{
    public PomodoroEngine(TimeSpan workDuration, TimeSpan breakDuration)
    {
        ValidateDuration(workDuration, nameof(workDuration));
        ValidateDuration(breakDuration, nameof(breakDuration));
        WorkDuration = workDuration;
        BreakDuration = breakDuration;
        Remaining = workDuration;
    }

    /// <summary>Raised whenever remaining time, phase, or running state changes.</summary>
    public event EventHandler? StateChanged;

    /// <summary>Raised when the timer automatically switches to the next phase.</summary>
    public event EventHandler<TimerPhase>? PhaseChanged;

    public TimeSpan WorkDuration { get; private set; }

    public TimeSpan BreakDuration { get; private set; }

    public TimerPhase Phase { get; private set; } = TimerPhase.Work;

    public TimeSpan Remaining { get; private set; }

    public bool IsRunning { get; private set; }

    private TimeSpan CurrentPhaseDuration => DurationOf(Phase);

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        OnStateChanged();
    }

    public void Pause()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        OnStateChanged();
    }

    /// <summary>Stops the timer and returns to the beginning of a Work phase using the latest durations.</summary>
    public void Reset()
    {
        IsRunning = false;
        Phase = TimerPhase.Work;
        Remaining = WorkDuration;
        OnStateChanged();
    }

    /// <summary>
    /// Changes the configured durations. The new values apply from the next phase; the interval in
    /// progress is left untouched unless the timer is idle at the very start of a phase.
    /// </summary>
    public void UpdateDurations(TimeSpan workDuration, TimeSpan breakDuration)
    {
        ValidateDuration(workDuration, nameof(workDuration));
        ValidateDuration(breakDuration, nameof(breakDuration));

        var isIdleAtPhaseStart = !IsRunning && Remaining == CurrentPhaseDuration;
        WorkDuration = workDuration;
        BreakDuration = breakDuration;

        if (isIdleAtPhaseStart)
        {
            Remaining = CurrentPhaseDuration;
            OnStateChanged();
        }
    }

    /// <summary>Advances the countdown by <paramref name="elapsed"/> when running.</summary>
    public void Tick(TimeSpan elapsed)
    {
        if (!IsRunning || elapsed <= TimeSpan.Zero)
        {
            return;
        }

        Remaining -= elapsed;
        while (Remaining <= TimeSpan.Zero)
        {
            var overshoot = -Remaining;
            Phase = Phase == TimerPhase.Work ? TimerPhase.Break : TimerPhase.Work;
            Remaining = CurrentPhaseDuration - overshoot;
            PhaseChanged?.Invoke(this, Phase);
        }

        OnStateChanged();
    }

    private TimeSpan DurationOf(TimerPhase phase) => phase == TimerPhase.Work ? WorkDuration : BreakDuration;

    private void OnStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);

    private static void ValidateDuration(TimeSpan duration, string paramName)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(paramName, duration, "Duration must be positive.");
        }
    }
}
