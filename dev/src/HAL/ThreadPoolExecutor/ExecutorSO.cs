using System;
using System.Threading;
using HAL.DllImportMethods;
using HAL.Loggin;
using HAL.OSData;
using HAL.Plugin;

namespace HAL.Executor.ThreadPoolExecutor
{
    public partial class ThreadPoolPluginExecutor : IPluginExecutor

    {
        /// <summary>
        ///     run a code from a shared object fille
        /// </summary>
        /// <param name="plugin">plugin to be executed</param>
        public void RunFromSO(APlugin plugin)
        {
            // .so works only on linux
            if (OSAttribute.IsLinux)
            {
                QueueLength++;

                ThreadPool.QueueUserWorkItem(obj =>
                {
                    try
                    {
                        using var dllimport = new DllImportEntryPoint();
                        var result = dllimport.UseRunEntryPointSharedObject(plugin.Infos.FilePath);

                        plugin.RaiseOnExecutionFinished(result);
                    }
                    catch (Exception e)
                    {
                        Log.Instance?.Error($"{plugin.Infos.FileName} encountered a problem: {e.Message}");
                    }
                    finally
                    {
                        Consume();
                    }
                });
            }
        }
    }
}