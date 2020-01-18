using HAL.Plugin;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class StorageServerFile : IStoragePlugin
    {
        public StreamWriter StreamWriter { get; set; }

        private DifferencialStorage diffStorage = new DifferencialStorage();

        public void Init(string connectionString)
        {
        }

        public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
        {
            if (!diffStorage.HasDifference(plugin, obj))
            {
                return StorageCode.Pass;
            }

            string strTodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            string completeTodayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            string path = $"{strTodayDate}/{Environment.MachineName}/{plugin.Infos.Name}";
            string fileName = $"{completeTodayDate}_{plugin.Infos.Name}.json";

            await StreamWriter.WriteLineAsync($"{path};{fileName};{obj.ToString()}");
            await StreamWriter.FlushAsync();

            return StorageCode.Success;
        }
        public void Dispose()
        {
        }
    }
}
