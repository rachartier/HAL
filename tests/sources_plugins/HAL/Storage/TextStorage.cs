using System;

namespace HAL.Storage
{
    public class TextStorage : IStorage
    {
        public StorageCode Save<T>(T obj)
        {
            Console.WriteLine(obj);
            return StorageCode.Success;
        }
    }
}
