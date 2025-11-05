using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Timetable.App.Models;

namespace Timetable.App.ViewModels;

public partial class TodaySlotViewModel : ObservableObject
{
    private readonly ScheduledSlot _slot;

    public string TimeRange => $"{_slot.TimeSlot.Start:hh\\:mm} - {_slot.TimeSlot.End:hh\\:mm}";
    public string SubjectName => _slot.Subject?.Name ?? "休息 / 自习";
    public TimeSpan StartTime => _slot.TimeSlot.Start;
    public TimeSpan EndTime => _slot.TimeSlot.End;
    public ScheduledSlot Model => _slot;

    [ObservableProperty]
    private bool _isCurrent;

    [ObservableProperty]
    private double _progressPercent;

    public TodaySlotViewModel(ScheduledSlot slot)
    {
        _slot = slot;
    }
}