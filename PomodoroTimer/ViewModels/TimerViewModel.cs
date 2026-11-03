using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using PomodoroTimer.Core;

namespace PomodoroTimer.ViewModels;

public sealed partial class TimerViewModel : ObservableObject
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(200);

    private readonly PomodoroEngine _engine;
    private readonly DispatcherQueueTimer _timer;
    private readonly Stopwatch _stopwatch = new();

    public TimerViewModel(PomodoroEngine engine, DispatcherQueue dispatcherQueue)
    {
        _engine = engine;
        _timer = dispatcherQueue.CreateTimer();
        _timer.Interval = TickInterval;
        _timer.IsRepeating = true;
        _timer.Tick += OnTimerTick;
        _engine.StateChanged += (_, _) => RefreshFromEngine();
        RefreshFromEngine();
    }

    [ObservableProperty]
    public partial string RemainingText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string PhaseText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusText { get; private set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    public partial bool IsRunning { get; private set; }

    private bool CanStart => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        _stopwatch.Restart();
        _timer.Start();
        _engine.Start();
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void Pause()
    {
        StopTimer();
        _engine.Pause();
    }

    [RelayCommand]
    private void Reset()
    {
        StopTimer();
        _engine.Reset();
    }

    private void OnTimerTick(DispatcherQueueTimer sender, object args)
    {
        var elapsed = _stopwatch.Elapsed;
        _stopwatch.Restart();
        _engine.Tick(elapsed);
    }

    private void StopTimer()
    {
        _timer.Stop();
        _stopwatch.Reset();
    }

    private void RefreshFromEngine()
    {
        RemainingText = TimeFormatter.Format(_engine.Remaining);
        PhaseText = _engine.Phase == TimerPhase.Work ? "Work" : "Break";
        IsRunning = _engine.IsRunning;
        StatusText = IsRunning
            ? "Running"
            : _engine.Phase == TimerPhase.Work && _engine.Remaining == _engine.WorkDuration ? "Ready" : "Paused";
    }
}
