using System;
using System.IO;
using System.Threading.Tasks;
using HAL.Plugin;

namespace HAL.Storage
{
    public class StorageLocalFile : IStoragePlugin
    {
        public string SavePath { get; set; }

        public void Init(string connectionString)
        {
        }

        public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
        {
            var dirName = "results/";

            var strTodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            var completeTodayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            var folder = $"{SavePath}/{dirName}{strTodayDate}/{plugin.Infos.Name}/";

            Directory.CreateDirectory(folder);

            using var fw = File.CreateText($"{folder}{completeTodayDate}_{plugin.Infos.Name}.json");
            await fw.WriteAsync(obj.ToString());

            return StorageCode.Success;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}