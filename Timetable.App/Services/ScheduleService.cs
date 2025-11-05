using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Timetable.App.Models;

namespace Timetable.App.Services;

public class ScheduleService : IScheduleService
{
    private readonly IConfigService _configService;
    private readonly Dictionary<string, Subject> _subjects = new();
    private readonly Dictionary<string, TimeSlot> _slots = new();
    private readonly Dictionary<string, Rotation> _rotations = new();

    public ScheduleService(IConfigService configService)
    {
        _configService = configService;
        _configService.ConfigReloaded += OnConfigReloaded;
        // Manually trigger the first cache build after subscribing.
        OnConfigReloaded(this, EventArgs.Empty);
    }

    private void OnConfigReloaded(object? sender, EventArgs e)
    {
        CacheConfig();
    }

    private void CacheConfig()
    {
        var config = _configService.Config;
        _subjects.Clear();
        _slots.Clear();
        _rotations.Clear();

        foreach (var subject in config.Subjects)
        {
            _subjects[subject.Id] = subject;
        }

        foreach (var slotConfig in config.Slots)
        {
            if (TimeSpan.TryParse(slotConfig.Start, out var start) && TimeSpan.TryParse(slotConfig.End, out var end))
            {
                _slots[slotConfig.Id] = new TimeSlot(slotConfig.Id, start, end);
            }
        }

        foreach (var rotationConfig in config.Rotations)
        {
            var startDate = rotationConfig.StartDate ?? config.Meta.StartDate;
            var skipDays = new List<DayOfWeek>();
            if (rotationConfig.SkipDays != null)
            {
                skipDays.AddRange(rotationConfig.SkipDays);
            }
            else if (rotationConfig.SkipWeekend)
            {
                skipDays.Add(DayOfWeek.Saturday);
                skipDays.Add(DayOfWeek.Sunday);
            }

            _rotations[rotationConfig.Name] = new Rotation(
                rotationConfig.Name,
                rotationConfig.Subjects,
                startDate,
                skipDays);
        }
    }

    public DaySchedule BuildSchedule(DateOnly date)
    {
        var schedule = new DaySchedule(date);
        foreach (var slot in _slots.Values.OrderBy(s => s.Start))
        {
            var entry = ResolveEntry(date, slot.Id);
            Subject? subject = null;
            if (entry != null)
            {
                var subjectId = entry.Subject ?? ResolveRotationSubject(entry.Rotation, date);
                if (subjectId != null && _subjects.TryGetValue(subjectId, out var foundSubject))
                {
                    subject = foundSubject;
                }
            }
            schedule.Slots.Add(new ScheduledSlot(slot, subject, entry?.Note));
        }
        return schedule;
    }

    public PlanEntry? ResolveEntry(DateOnly date, string slotId)
    {
        var config = _configService.Config;

        // Priority 1: Date Overrides
        var dateOverride = config.DateOverrides.FirstOrDefault(o => o.Date == date);
        if (dateOverride != null)
        {
            if (dateOverride.AllDayOff)
            {
                return null; // All day off
            }
            if (dateOverride.Slots != null && dateOverride.Slots.TryGetValue(slotId, out var overriddenEntry))
            {
                return overriddenEntry;
            }
        }

        // Priority 2: Weekly Plan
        var dayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
        if (config.WeeklyPlan.TryGetValue(dayOfWeek, out var dayPlan) && dayPlan.TryGetValue(slotId, out var weeklyEntry))
        {
            return weeklyEntry;
        }

        return null; // No entry for this slot
    }

    private string? ResolveRotationSubject(string? rotationName, DateOnly targetDate)
    {
        if (string.IsNullOrEmpty(rotationName) || !_rotations.TryGetValue(rotationName, out var rotation))
        {
            return null;
        }

        var startDate = rotation.StartDate;
        int n = CountEffectiveDays(startDate, targetDate, rotation.SkipDays);

        if (n < 0) n = 0; // If target is before start date, use the first subject

        if (rotation.Subjects.Count == 0) return null;

        return rotation.Subjects[n % rotation.Subjects.Count];
    }

    private static int CountEffectiveDays(DateOnly from, DateOnly to, List<DayOfWeek> skipDays)
    {
        if (from >= to) return 0;
        int days = 0;
        for (var d = from; d < to; d = d.AddDays(1))
        {
            if (!skipDays.Contains(d.DayOfWeek))
            {
                days++;
            }
        }
        return days;
    }
}