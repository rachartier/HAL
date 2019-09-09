using System;
using System.Collections.Generic;
using System.Threading;

namespace HAL.Plugin.Schelduler
{
    public class ScheldulerService
    {
        public static ScheldulerService Instance => instance ?? (instance = new ScheldulerService());
        private static ScheldulerService instance;

        private IDictionary<string, Timer> timers = new Dictionary<string, Timer>();

        private ScheldulerService()
        {
        }

        /// <summary>
        /// scheldule a task, it will be repeated once in a hearthbeat
        /// </summary>
        /// <param name="taskName">task's name to be identified</param>
        /// <param name="intervalHours">hearthbeat in hours, meaning it will be repeated at hearthbeats per hour</param>
        /// <param name="task">the specific task</param>
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

        /// <summary>
        /// unscheldule a task
        /// </summary>
        /// <param name="taskName">task's name identifier</param>
        /// <returns></returns>
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
}