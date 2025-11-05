using System;
using System.Collections.Generic;

namespace Timetable.App.Models;

public class Rotation
{
    public string Name { get; set; }
    public List<string> Subjects { get; set; }
    public DateOnly StartDate { get; set; }
    public List<DayOfWeek> SkipDays { get; set; }

    public Rotation(string name, List<string> subjects, DateOnly startDate, List<DayOfWeek> skipDays)
    {
        Name = name;
        Subjects = subjects;
        StartDate = startDate;
        SkipDays = skipDays;
    }
}