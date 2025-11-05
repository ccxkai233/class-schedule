using System;
using System.Windows;
using System.Windows.Threading;

namespace Timetable.App.Views;

/// <summary>
/// Interaction logic for ReminderWindow.xaml
/// </summary>
public partial class ReminderWindow : Window
{
    private readonly DispatcherTimer _closeTimer;

    public ReminderWindow()
    {
        InitializeComponent();
        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
        {
            Owner = Application.Current.MainWindow;
        }

        _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        _closeTimer.Tick += (_, _) => Close();
        _closeTimer.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _closeTimer?.Stop();
        base.OnClosed(e);
    }
}