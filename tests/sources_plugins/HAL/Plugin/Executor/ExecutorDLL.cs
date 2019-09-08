using HAL.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace HAL.Plugin.Executor
{
    public partial class PluginExecutor
    {
        public void RunFromDLL(PluginFile plugin, IStorage storage)
        {
            QueueLength++;

            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
            {
                try
                {
                    var assembly = Assembly.LoadFrom(plugin.FilePath);
                    var type = assembly.GetTypes().FirstOrDefault();

                    var entryPointMethod = type?.GetMethod(MethodEntryPointName);

                    if (entryPointMethod != null)
                    {
                        dynamic instance = Activator.CreateInstance(type);
                        dynamic result = entryPointMethod.Invoke(instance, null);

                        storage.Save(result);
                    }
                    else
                    {
                        throw new MethodAccessException($"Method 'Run' from DLL {plugin.FileName} not found.");
                    }
                }
                catch (Exception e)
                {
                    // if it can't load assembly, then it need to try a classic type file dll
                    if (e is System.BadImageFormatException || e is System.DllNotFoundException)
                    {
                        var result = UseRunEntryPointSharedObject(plugin.FilePath);

                        storage.Save(result);
                    }
                }

                Consume();
            }));
        }
    }
}
