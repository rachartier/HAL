using HAL.Plugin;
using System;

namespace HAL.Executor
{
    public interface IPluginExecutor
    {
        string MethodEntryPointName { get; set; }

        void RunFromDLL(APlugin plugin);

        void RunFromSO(APlugin plugin);

        void RunFromScript(APlugin plugin);
    }
}