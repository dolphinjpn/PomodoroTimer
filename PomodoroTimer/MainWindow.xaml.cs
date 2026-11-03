using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PomodoroTimer.Core;
using PomodoroTimer.Pages;
using PomodoroTimer.ViewModels;
using Windows.Graphics;

namespace PomodoroTimer;

public sealed partial class MainWindow : Window
{
    // Rubric: nav pane (~320) + content (~460 incl. padding); title bar + timer content + padding.
    private const int WindowWidthDip = 800;
    private const int WindowHeightDip = 600;

    public MainWindow(SettingsViewModel settingsViewModel)
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");
        ResizeToDefault();

        ApplyTheme(settingsViewModel.Theme);
        settingsViewModel.ThemeChanged += (_, theme) => ApplyTheme(theme);

        NavView.SelectedItem = TimerNavItem;
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    private void ResizeToDefault()
    {
        var hwnd = Win32Interop.GetWindowFromWindowId(AppWindow.Id);
        var scale = GetDpiForWindow(hwnd) / 96.0;
        AppWindow.Resize(new SizeInt32((int)(WindowWidthDip * scale), (int)(WindowHeightDip * scale)));
    }

    private void ApplyTheme(AppTheme theme)
    {
        RootGrid.RequestedTheme = theme == AppTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
        AppWindow.TitleBar.PreferredTheme = theme == AppTheme.Dark ? TitleBarTheme.Dark : TitleBarTheme.Light;
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var target = args.IsSettingsSelected ? typeof(SettingsPage) : typeof(TimerPage);
        if (NavFrame.CurrentSourcePageType != target)
        {
            NavFrame.Navigate(target);
        }
    }
}
