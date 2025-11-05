using System;
using System.Collections.Generic;

namespace Timetable.App.Models;

public class Rotation
{
    public string Name { get; set; }
    public List<string> Subjects { get; set; }
    public DateOnly StartDate { get; set; }
    public bool SkipWeekend { get; set; }

    public Rotation(string name, List<string> subjects, DateOnly startDate, bool skipWeekend)
    {
        Name = name;
        Subjects = subjects;
        StartDate = startDate;
        SkipWeekend = skipWeekend;
    }
}