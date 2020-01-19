using HAL.Plugin;
using System;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class StorageText : DifferencialStorage
    {
        public override async Task<StorageCode> SaveDifferencial<T>(APlugin plugin, T obj)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(obj);
            });

            return StorageCode.Success;
        }
    }
}