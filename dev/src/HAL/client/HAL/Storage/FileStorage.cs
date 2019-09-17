using System;
using System.IO;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class FileStorage : IStoragePlugin
    {
        public async Task<StorageCode> Save<T>(Plugin.BasePlugin plugin, T obj)
        {
            string dirName = "results/";

            string strTodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            string completeTodayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            Directory.CreateDirectory($"{dirName}{strTodayDate}/{plugin.Name}/");

            using (var fw = File.CreateText($"{dirName}{strTodayDate}/{plugin.Name}/{completeTodayDate}_{plugin.Name}.json"))
            {
                await fw.WriteAsync(obj.ToString());
            }

            return StorageCode.Success;
        }
    }
}
