using System;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class TextStorage : IStoragePlugin
    {
        public void Init()
        {
        }
        public async Task<StorageCode> Save<T>(Plugin.BasePlugin plugin, T obj)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(obj);
            });

            return StorageCode.Success;
        }
    }
}
