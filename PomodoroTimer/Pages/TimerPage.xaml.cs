using Microsoft.UI.Xaml.Controls;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Pages;

public sealed partial class TimerPage : Page
{
    public TimerPage()
    {
        InitializeComponent();
    }

    public TimerViewModel ViewModel { get; } = App.Current.TimerViewModel;
}
