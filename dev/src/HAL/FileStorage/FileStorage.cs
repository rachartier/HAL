using HAL.Plugin;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public class FileStorage : IStoragePlugin
    {
        public void Init()
        {
        }

        public async Task<StorageCode> Save<T>(APlugin plugin, T obj)
        {
            string dirName = "results/";

            string strTodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            string completeTodayDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            Directory.CreateDirectory($"{dirName}{strTodayDate}/{plugin.Infos.Name}/");

            using (var fw = File.CreateText($"{dirName}{strTodayDate}/{plugin.Infos.Name}/{completeTodayDate}_{plugin.Infos.Name}.json"))
            {
                await fw.WriteAsync(obj.ToString());
            }

            return StorageCode.Success;
        }
    }
}