using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAL.Loggin;

namespace HAL.Scheduler
{
    public class SchedulerService
    {
        private static uint NB_MILLIS_IN_MINUTE = 60_000;
        private static uint MAX_RETRY = 10;

        private static SchedulerService instance;

        private readonly IDictionary<string, Timer> timers = new Dictionary<string, Timer>();

        private SchedulerService()
        {
        }

        public static SchedulerService Instance => instance ??= new SchedulerService();

        /// <summary>
        ///     schedule a task, it will be repeated each interval
        /// </summary>
        /// <param name="taskName">task's name to be identified</param>
        /// <param name="interval">interval of the task to be repeated</param>
        /// <param name="task">the specific task</param>
        public void ScheduleTask(string taskName, double interval, Action task)
        {
            var timer = new Timer(t => { task.Invoke(); }, null, 0, (uint) (interval * NB_MILLIS_IN_MINUTE));

            if (timers.TryAdd(taskName, timer) == false) throw new ArgumentException("Task name already in use.");

            Log.Instance?.Info($"{taskName} scheduled each {interval} heartbeats.");
        }

        /// <summary>
        ///     unschedule a task
        /// </summary>
        /// <param name="taskName">task's name identifier</param>
        /// <returns></returns>
        public bool UnscheduleTask(string taskName)
        {
            if (timers.TryGetValue(taskName, out var timer))
            {
                timer.Dispose();

                Log.Instance?.Info($"{taskName} unscheduled");

                return timers.Remove(taskName);
            }

            return false;
        }

        /// <summary>
        ///     unschedule a task asynchronously
        /// </summary>
        /// <param name="taskName">task's name identifier</param>
        /// <returns></returns>
        public async Task<bool> UnscheduleTaskAsync(string taskName)
        {
            if (timers.TryGetValue(taskName, out var timer))
            {
                int retry = 0;
                
                await timer.DisposeAsync();

                while (timers.Remove(taskName) && retry < MAX_RETRY)
                {
                    await Task.Delay(100);
                    retry++;
                }

                Log.Instance?.Info($"{taskName} unscheduled");
                return true;
            }

            Log.Instance?.Error($"{taskName} not found.");
            return false;
        }
    }
}