using HAL.Plugin;
using System;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class StorageText : IStoragePlugin
    {
        public void Init(string connectionString)
        {
        }

        public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(obj);
            });

            return StorageCode.Success;
        }
    }
}