using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using Timetable.App.Services;

namespace Timetable.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public TodayViewModel TodayViewModel { get; }

    public MainViewModel()
    {
        // In a real application with dependency injection, these services would be injected.
        // For simplicity here, we'll new them up. This will be refined later.
        IConfigService configService = new ConfigService();
        var configPath = Path.Combine(AppContext.BaseDirectory, "config/timetable.yaml");
        configService.Load(configPath);

        IScheduleService scheduleService = new ScheduleService(configService);
        ISpeechService speechService = new SpeechService();
        INotificationService notificationService = new NotificationService();
        TimerService timerService = new TimerService();

        TodayViewModel = new TodayViewModel(scheduleService, notificationService, speechService, timerService, configService);
    }
}