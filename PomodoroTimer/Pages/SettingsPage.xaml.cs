using Microsoft.UI.Xaml.Controls;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; } = App.Current.SettingsViewModel;
}
