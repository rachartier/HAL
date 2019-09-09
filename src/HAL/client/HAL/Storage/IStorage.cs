namespace HAL.Storage
{
    public interface IStorage
    {
        /// <summary>
        /// method to save an object
        /// </summary>
        /// <typeparam name="T">an object type</typeparam>
        /// <param name="obj">an object</param>
        /// <returns></returns>
        StorageCode Save<T>(T obj);
    }
}