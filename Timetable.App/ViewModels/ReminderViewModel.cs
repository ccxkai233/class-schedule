using CommunityToolkit.Mvvm.ComponentModel;

namespace Timetable.App.ViewModels;

public partial class ReminderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _message;

    public ReminderViewModel(string title, string message)
    {
        _title = title;
        _message = message;
    }
}