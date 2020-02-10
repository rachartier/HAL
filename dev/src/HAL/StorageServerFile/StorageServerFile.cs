using System;
using System.IO;
using System.Threading.Tasks;
using HAL.Plugin;

namespace HAL.Storage
{
    public class StorageServerFile : DifferencialStorage
    {
        public StreamWriter StreamWriter { get; set; }

        public override async Task<StorageCode> SaveDifferencial<T>(APlugin plugin, T obj)
        {
            var strTodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            var completeTodayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.ff");

            var path = $"{strTodayDate}/{Environment.MachineName}/{plugin.Infos.Name}";
            var fileName = $"{completeTodayDate}_{plugin.Infos.Name}.json";

            await StreamWriter.WriteLineAsync($"{path};{fileName};{obj.ToString()}");
            await StreamWriter.FlushAsync();

            return StorageCode.Success;
        }
    }
}