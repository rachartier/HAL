using System;
using HAL.Plugin;

namespace HAL.Executor
{
    public interface IPluginExecutor
    {
        string MethodEntryPointName { get; set; }
        event EventHandler OnAllPluginsExecuted;

        void RunFromDLL(APlugin plugin);

        void RunFromSO(APlugin plugin);

        void RunFromScript(APlugin plugin);
    }
}