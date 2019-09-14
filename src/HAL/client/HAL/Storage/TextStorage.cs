using HAL.Plugin;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class TextStorage : IStoragePlugin
    {
        public async Task<StorageCode> Save<T>(PluginFile plugin, T obj)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(obj);
            });

            return StorageCode.Success;
        }
    }
}
