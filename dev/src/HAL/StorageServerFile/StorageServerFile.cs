using HAL.Plugin;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class StorageServerFile : DifferencialStorage
    {
        public StreamWriter StreamWriter { get; set; }

        public override async Task<StorageCode> SaveDifferencial<T>(APlugin plugin, T obj)
        {
            string strTodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            string completeTodayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.ff");

            string path = $"{strTodayDate}/{Environment.MachineName}/{plugin.Infos.Name}";
            string fileName = $"{completeTodayDate}_{plugin.Infos.Name}.json";

            await StreamWriter.WriteLineAsync($"{path};{fileName};{obj.ToString()}");
            await StreamWriter.FlushAsync();

            return StorageCode.Success;
        }
    }
}
