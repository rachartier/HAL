using HAL.Loggin;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HAL.Scheduler
{
    public class SchedulerService
    {
        public static uint NB_MILLIS_IN_HOURS = 3_600_000U;

        public static SchedulerService Instance => instance ?? (instance = new SchedulerService());
        private static SchedulerService instance;

        private readonly IDictionary<string, Timer> timers = new Dictionary<string, Timer>();

        private SchedulerService()
        {
        }

        /// <summary>
        /// schedule a task, it will be repeated each interval
        /// </summary>
        /// <param name="taskName">task's name to be identified</param>
        /// <param name="intervalHours">interval hours, meaning the task will be repeated at an interval</param>
        /// <param name="task">the specific task</param>
        public void ScheduleTask(string taskName, double intervalHours, Action task)
        {
            var timer = new Timer(t =>
            {
                task.Invoke();
            }, null, 0, (uint)(intervalHours * NB_MILLIS_IN_HOURS));

            if (timers.TryAdd(taskName, timer) == false)
            {
                throw new ArgumentException("Task name already in use.");
            }

            Log.Instance?.Info($"{taskName} scheduled each {intervalHours} heartbeats.");
        }

        /// <summary>
        /// unschedule a task
        /// </summary>
        /// <param name="taskName">task's name identifier</param>
        /// <returns></returns>
        public bool UnscheduleTask(string taskName)
        {
            if (timers.TryGetValue(taskName, out Timer timer))
            {
                timer.Dispose();

                Log.Instance?.Info($"{taskName} unscheduled");

                return timers.Remove(taskName);
            }
            return false;
        }
    }
}