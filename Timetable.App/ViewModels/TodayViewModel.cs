using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Timetable.App.Models;
using Timetable.App.Services;

namespace Timetable.App.ViewModels;

public partial class TodayViewModel : ObservableObject
{
    private readonly IScheduleService _scheduleService;
    private readonly INotificationService _notificationService;
    private readonly ISpeechService _speechService;
    private readonly TimerService _timerService;
    private readonly IConfigService _configService;
    private readonly Action? _showWindowAction;

    public ObservableCollection<TodaySlotViewModel> TodaySlots { get; } = new();

    [ObservableProperty]
    private string? _nowDateText;

    [ObservableProperty]
    private string? _nowTimeText;

    [ObservableProperty]
    private string? _currentSubjectName;

    [ObservableProperty]
    private string? _countdownText;

    [ObservableProperty]
    private string? _nextSubjectText;

    [ObservableProperty]
    private TodaySlotViewModel? _currentSlot;

    private readonly HashSet<Tuple<string, TimeSpan>> _firedReminders = new();
    private DateTime _lastTick;

    public TodayViewModel(
        IScheduleService scheduleService,
        INotificationService notificationService,
        ISpeechService speechService,
        TimerService timerService,
        IConfigService configService,
        Action? showWindowAction = null)
    {
        _scheduleService = scheduleService;
        _notificationService = notificationService;
        _speechService = speechService;
        _timerService = timerService;
        _configService = configService;
        _showWindowAction = showWindowAction;

        _configService.ConfigReloaded += (_, _) => RebuildTodaySchedule();
        _timerService.Tick += OnTick;

        _lastTick = DateTime.Now;
        RebuildTodaySchedule();
        _timerService.Start();
    }

    private void RebuildTodaySchedule()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var schedule = _scheduleService.BuildSchedule(today);

        TodaySlots.Clear();
        foreach (var slot in schedule.Slots)
        {
            TodaySlots.Add(new TodaySlotViewModel(slot));
        }
        
        _firedReminders.Clear(); // Reset reminders when config reloads
        OnTick(null, EventArgs.Empty); // Immediately update UI
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        NowDateText = now.ToString("yyyy-MM-dd dddd");
        NowTimeText = now.ToString("HH:mm:ss");

        var currentTimeOfDay = now.TimeOfDay;
        var newCurrentSlot = TodaySlots.FirstOrDefault(s => currentTimeOfDay >= s.StartTime && currentTimeOfDay < s.EndTime);

        if (CurrentSlot != newCurrentSlot)
        {
            if (CurrentSlot != null) CurrentSlot.IsCurrent = false;
            CurrentSlot = newCurrentSlot;
            if (CurrentSlot != null) CurrentSlot.IsCurrent = true;
        }

        // Update progress for all slots
        foreach (var slot in TodaySlots)
        {
            if (currentTimeOfDay >= slot.EndTime)
            {
                slot.ProgressPercent = 100;
            }
            else if (currentTimeOfDay >= slot.StartTime && currentTimeOfDay < slot.EndTime)
            {
                var totalDuration = slot.EndTime - slot.StartTime;
                var elapsed = currentTimeOfDay - slot.StartTime;
                slot.ProgressPercent = totalDuration.TotalSeconds > 0 ? (elapsed.TotalSeconds / totalDuration.TotalSeconds) * 100 : 0;
            }
            else
            {
                slot.ProgressPercent = 0;
            }
        }

        // Update current slot specific info
        if (CurrentSlot != null)
        {
            CurrentSubjectName = CurrentSlot.SubjectName;
            var remaining = CurrentSlot.EndTime - currentTimeOfDay;
            CountdownText = $"-{remaining:mm\\:ss}";
            
            var nextSlot = TodaySlots.FirstOrDefault(s => s.StartTime >= CurrentSlot.EndTime && s.Model.Subject != null);
            NextSubjectText = nextSlot != null ? $"下一节: {nextSlot.SubjectName} ({nextSlot.TimeRange})" : "今天课程已结束";
        }
        else
        {
            CurrentSubjectName = "空闲时间";
            var nextSlot = TodaySlots.FirstOrDefault(s => s.StartTime > currentTimeOfDay);
            if (nextSlot != null)
            {
                var remaining = nextSlot.StartTime - currentTimeOfDay;
                CountdownText = $"至下节课: {remaining:hh\\:mm\\:ss}";
            }
            else
            {
                CountdownText = string.Empty;
            }
            NextSubjectText = FindNextUpcomingSubjectText(currentTimeOfDay);
        }
        
        CheckAndFireReminders(now);
        _lastTick = now;
    }

    private string FindNextUpcomingSubjectText(TimeSpan currentTimeOfDay)
    {
        var nextSlot = TodaySlots.FirstOrDefault(s => s.StartTime > currentTimeOfDay && s.Model.Subject != null);
        return nextSlot != null ? $"下一节: {nextSlot.SubjectName} ({nextSlot.TimeRange})" : "今天课程已结束";
    }

    private void CheckAndFireReminders(DateTime now)
    {
        var meta = _configService.Config.Meta;

        foreach (var slot in TodaySlots)
        {
            if (slot.Model.Subject == null) continue;

            // Check for pre-reminders (before start)
            foreach (var minute in meta.PreAlertMinutes)
            {
                var reminderTime = slot.StartTime.Subtract(TimeSpan.FromMinutes(minute));
                var reminderKey = Tuple.Create($"pre_{slot.Model.TimeSlot.Id}", reminderTime);

                if (now.TimeOfDay >= reminderTime && _lastTick.TimeOfDay < reminderTime && !_firedReminders.Contains(reminderKey))
                {
                    FireReminder(slot, minute, isStart: true);
                    _firedReminders.Add(reminderKey);
                }
            }

            // Check for post-reminders (after end)
            foreach (var minute in meta.PostAlertMinutes)
            {
                var reminderTime = slot.EndTime.Add(TimeSpan.FromMinutes(minute));
                var reminderKey = Tuple.Create($"post_{slot.Model.TimeSlot.Id}", reminderTime);

                if (now.TimeOfDay >= reminderTime && _lastTick.TimeOfDay < reminderTime && !_firedReminders.Contains(reminderKey))
                {
                    FireReminder(slot, minute, isStart: false);
                    _firedReminders.Add(reminderKey);
                }
            }
        }
    }

    private void FireReminder(TodaySlotViewModel slot, int minute, bool isStart)
    {
        string title;
        string message;
        string speechText;

        var nextSlot = TodaySlots.FirstOrDefault(s => s.StartTime > slot.StartTime && s.Model.Subject != null);
        var nextSubjectInfo = nextSlot != null ? $"下一节是{nextSlot.SubjectName}。" : "今天课程已结束。";

        if (isStart)
        {
            title = minute == 0 ? $"课程开始: {slot.SubjectName}" : $"即将开始: {slot.SubjectName}";
            message = $"时间: {slot.TimeRange}";
            speechText = minute == 0
                ? $"现在开始{slot.SubjectName}。"
                : $"{minute}分钟后，将开始{slot.SubjectName}。";
        }
        else
        {
            title = $"课程结束: {slot.SubjectName}";
            message = nextSubjectInfo;
            speechText = $"{slot.SubjectName}已结束。{nextSubjectInfo}";
        }

        _notificationService.ShowInAppReminder(title, message);

        if (_configService.Config.Meta.ShowWindowOnReminder)
        {
            _showWindowAction?.Invoke();
        }

        if (_speechService is SpeechService speechServiceImpl)
        {
            speechServiceImpl.SelectVoice(_configService.Config.Meta.SpeechVoice);
        }
        _speechService.SpeakAsync(speechText);
    }
}