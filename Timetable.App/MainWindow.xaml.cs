using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Timetable.App.ViewModels;
using Forms = System.Windows.Forms;

namespace Timetable.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private bool _isExplicitClose;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(ShowAndActivate);

        _notifyIcon = new Forms.NotifyIcon();
        var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/icon.ico"))?.Stream;
        if (iconStream != null)
        {
            _notifyIcon.Icon = new System.Drawing.Icon(iconStream);
        }
        _notifyIcon.Text = "电子课程表";
        _notifyIcon.MouseClick += NotifyIcon_MouseClick;

        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add("显示", null, ShowWindow_Click);
        contextMenu.Items.Add("退出", null, Exit_Click);
        _notifyIcon.ContextMenuStrip = contextMenu;

    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExplicitClose)
        {
            e.Cancel = true;
            Hide();
            _notifyIcon.Visible = true;
        }
        base.OnClosing(e);
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        _isExplicitClose = true;
        _notifyIcon.Dispose();
        Close();
    }

    private void ShowWindow_Click(object? sender, EventArgs e)
    {
        ShowAndActivate();
    }


    private void NotifyIcon_MouseClick(object? sender, Forms.MouseEventArgs e)
    {
        if (e.Button == Forms.MouseButtons.Left)
        {
            // A simple left-click on the icon will also show the window.
            ShowAndActivate();
        }
    }

    private void ShowAndActivate()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _notifyIcon.Visible = false;
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}