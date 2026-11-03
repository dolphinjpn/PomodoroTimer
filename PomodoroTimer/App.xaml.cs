using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using PomodoroTimer.Core;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    public static new App Current => (App)Application.Current;

    public TimerViewModel TimerViewModel { get; private set; } = null!;

    public SettingsViewModel SettingsViewModel { get; private set; } = null!;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var store = LocalSettingsStore.CreateForCurrentProcess();
        var settings = store.Load();
        var engine = new PomodoroEngine(
            TimeSpan.FromMinutes(settings.WorkMinutes),
            TimeSpan.FromMinutes(settings.BreakMinutes));

        TimerViewModel = new TimerViewModel(engine, DispatcherQueue.GetForCurrentThread());
        SettingsViewModel = new SettingsViewModel(store, engine, settings);

        _window = new MainWindow(SettingsViewModel);
        _window.Activate();
    }
}
