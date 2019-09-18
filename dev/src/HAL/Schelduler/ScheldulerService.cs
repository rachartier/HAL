using HAL.Loggin;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HAL.Plugin.Schelduler
{
    public class ScheldulerService
    {
        public static uint NB_MILLIS_IN_HOURS = 3_600_000U;

        public static ScheldulerService Instance => instance ?? (instance = new ScheldulerService());
        private static ScheldulerService instance;

        private readonly IDictionary<string, Timer> timers = new Dictionary<string, Timer>();

        private ScheldulerService()
        {
        }

        /// <summary>
        /// scheldule a task, it will be repeated each interval
        /// </summary>
        /// <param name="taskName">task's name to be identified</param>
        /// <param name="intervalHours">interval hours, meaning the task will be repeated at an interval</param>
        /// <param name="task">the specific task</param>
        public void SchelduleTask(string taskName, double intervalHours, Action task)
        {
            var timer = new Timer(t =>
            {
                task.Invoke();
            }, null, 0, (uint)(intervalHours * NB_MILLIS_IN_HOURS));

            if (timers.TryAdd(taskName, timer) == false)
            {
                throw new ArgumentException("Task name already in use.");
            }

            Log.Instance?.Info($"{taskName} schelduled each {intervalHours} heartbeats.");
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

                Log.Instance?.Info($"{taskName} unschelduled");

                return timers.Remove(taskName);
            }
            return false;
        }
    }
}