using System;
using HAL.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SchedulerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Scheduler_ValidAddTask()
        {
            try
            {
                SchedulerService.Instance.ScheduleTask("task_test_1", 1, () => { });
            }
            catch (Exception e)
            {
                SchedulerService.Instance.UnscheduleTask("task_test_1");

                Assert.Fail($"ScheduleTask shouldn't raise an exception: {e.Message}");
            }
        }

        [TestMethod]
        public void Scheduler_InvalidAddTask()
        {
            try
            {
                SchedulerService.Instance.ScheduleTask("task_test_2", 1, () => { });
                SchedulerService.Instance.ScheduleTask("task_test_2", 1, () => { });

                Assert.Fail("Scheduler should raise an exception (same task name)");
            }
            catch (Exception)
            {
                SchedulerService.Instance.UnscheduleTask("task_test_2");
            }
        }
    }
}