using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Windows;
using Timetable.App.ViewModels;
using Timetable.App.Views;

namespace Timetable.App.Services;

public class NotificationService : INotificationService
{
    public void ShowInAppReminder(string title, string message)
    {
        // We need to dispatch this to the UI thread.
        Application.Current?.Dispatcher.Invoke(() =>
        {
            var reminder = new ReminderWindow
            {
                DataContext = new ReminderViewModel(title, message)
            };
            reminder.Show();
        });
    }

    public void ShowToast(string title, string message)
    {
        // new ToastContentBuilder()
        //     .AddText(title)
        //     .AddText(message)
        //     .Show();
    }
}