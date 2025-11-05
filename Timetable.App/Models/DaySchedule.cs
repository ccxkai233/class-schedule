using System;
using System.Collections.Generic;

namespace Timetable.App.Models;

public class DaySchedule
{
    public DateOnly Date { get; }
    public List<ScheduledSlot> Slots { get; } = new();

    public DaySchedule(DateOnly date)
    {
        Date = date;
    }
}

public class ScheduledSlot
{
    public TimeSlot TimeSlot { get; }
    public Subject? Subject { get; }
    public string? Note { get; }

    public ScheduledSlot(TimeSlot timeSlot, Subject? subject = null, string? note = null)
    {
        TimeSlot = timeSlot;
        Subject = subject;
        Note = note;
    }
}