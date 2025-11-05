using System;

namespace Timetable.App.Models;

public class TimeSlot
{
    public string Id { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }

    public TimeSlot(string id, TimeSpan start, TimeSpan end)
    {
        Id = id;
        Start = start;
        End = end;
    }
}