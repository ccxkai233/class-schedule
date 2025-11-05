using System;
using Timetable.App.Models;

namespace Timetable.App.Services;

public interface IConfigService
{
    AppConfig Config { get; }
    event EventHandler? ConfigReloaded;
    void Load(string path);
}