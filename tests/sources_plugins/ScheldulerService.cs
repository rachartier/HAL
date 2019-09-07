using System;
using System.Collections.Generic;
using System.Threading;

public class ScheldulerService
{
    public static ScheldulerService Instance => instance ?? (instance = new ScheldulerService());
    private static ScheldulerService instance;

    private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

    private ScheldulerService()
    {
    }

	// intervalHours sera le hearthbeat
    public void SchelduleTask(string taskName, double intervalHours, Action task)
    {
        Func<double, int> hoursToMillis = x => (int)(intervalHours * 3_600_000);

        var timer = new Timer(t =>
        {
            task.Invoke();
        }, null, 0, hoursToMillis(intervalHours));

        if (timers.TryAdd(taskName, timer) == false)
        {
            throw new ArgumentException("Task name already in use.");
        }
    }

    public bool UnschelduleTask(string taskName)
    {
        if (timers.TryGetValue(taskName, out Timer timer))
        {
            timer.Dispose();

            return timers.Remove(taskName);
        }
        return false;
    }
}
