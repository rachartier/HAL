using System.IO;
using System;

using HAL.MagicString;
using System.Threading.Tasks;
using HAL.Plugin;

namespace HAL.Storage
{
    public class StorageServerFile : IStoragePlugin
    {
        public StreamWriter StreamWriter { get; set; }

        public void Init(string connectionString)
        {
        }

        public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
        {
            string strTodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            string completeTodayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            string path = $"{strTodayDate}/{Environment.MachineName}/{plugin.Infos.Name}";
            string fileName = $"{completeTodayDate}_{plugin.Infos.Name}.json";

            await StreamWriter.WriteLineAsync($"{path}\n{fileName}\n{obj.ToString()}");
            await StreamWriter.FlushAsync();
            
            return StorageCode.Success;
        }
    }
}
