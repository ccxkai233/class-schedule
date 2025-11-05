using System.Collections.Generic;

namespace Timetable.App.Models;

public class AppConfig
{
    public Meta Meta { get; set; } = new();
    public List<Subject> Subjects { get; set; } = new();
    public List<TimeSlotConfig> Slots { get; set; } = new();
    public List<RotationConfig> Rotations { get; set; } = new();
    public Dictionary<int, Dictionary<string, PlanEntry>> WeeklyPlan { get; set; } = new();
    public List<DateOverride> DateOverrides { get; set; } = new();
}

public class Meta
{
    public string Timezone { get; set; } = "Asia/Shanghai";
    public string Theme { get; set; } = "dark";
    public string AccentColor { get; set; } = "#5C6BC0";
    public DateOnly StartDate { get; set; } = new(2025, 1, 1);
    public string SpeechVoice { get; set; } = "ZH-CN";
    public List<int> PreAlertMinutes { get; set; } = new() { 5, 0 };
    public List<int> PostAlertMinutes { get; set; } = new() { 0 };
    public bool ShowWindowOnReminder { get; set; } = true;
}

public class TimeSlotConfig
{
    public string Id { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}

public class RotationConfig
{
    public string Name { get; set; } = string.Empty;
    public List<string> Subjects { get; set; } = new();
    public DateOnly? StartDate { get; set; }
    public bool SkipWeekend { get; set; }
}

public class PlanEntry
{
    public string? Subject { get; set; }
    public string? Rotation { get; set; }
    public string? Note { get; set; }
}

public class DateOverride
{
    public DateOnly Date { get; set; }
    public bool AllDayOff { get; set; }
    public Dictionary<string, PlanEntry>? Slots { get; set; }
}