using System;
using System.Windows.Threading;

namespace Timetable.App.Services;

public class TimerService
{
    private readonly DispatcherTimer _timer;
    public event EventHandler? Tick;

    public TimerService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => Tick?.Invoke(this, EventArgs.Empty);
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }
}