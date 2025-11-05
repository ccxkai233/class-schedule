namespace Timetable.App.Services;

public interface INotificationService
{
    void ShowInAppReminder(string title, string message);
    void ShowToast(string title, string message); // Optional
}