using HAL.Plugin.Schelduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ScheldulerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Schelduler_ValidAddTask()
        {
            try
            {
                ScheldulerService.Instance.SchelduleTask("task_test_1", 1, () => { });
            }
            catch (Exception e)
            {
                ScheldulerService.Instance.UnschelduleTask("task_test_1");

                Assert.Fail($"SchelduleTask shouldn't raise an exception: {e.Message}");
            }
        }

        [TestMethod]
        public void Schelduler_InvalidAddTask()
        {
            try
            {
                ScheldulerService.Instance.SchelduleTask("task_test_2", 1, () => { });
                ScheldulerService.Instance.SchelduleTask("task_test_2", 1, () => { });

                Assert.Fail("Schelduler should raise an exception (same task name)");
            }
            catch (Exception)
            {
                ScheldulerService.Instance.UnschelduleTask("task_test_2");
            }
        }
    }
}
