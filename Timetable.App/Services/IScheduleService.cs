using System;
using Timetable.App.Models;

namespace Timetable.App.Services;

public interface IScheduleService
{
    DaySchedule BuildSchedule(DateOnly date);
    PlanEntry? ResolveEntry(DateOnly date, string slotId);
}