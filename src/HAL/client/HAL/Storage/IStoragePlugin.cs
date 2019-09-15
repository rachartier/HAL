using HAL.Plugin;
using System.Threading.Tasks;

namespace HAL.Storage
{
    public interface IStoragePlugin
    {
        /// <summary>
        /// method to save an object
        /// </summary>
        /// <typeparam name="T">an object type</typeparam>
        /// <param name="plugin">plugin to be saved</param>
        /// <param name="obj">an object</param>
        /// <returns></returns>
        Task<StorageCode> Save<T>(PluginFile plugin, T obj);
    }
}