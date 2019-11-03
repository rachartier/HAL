using System;
using System.Collections;
using System.Collections.Generic;
using HAL.Storage;

namespace HAL.Factory
{
    public class StorageFactory
    {
        private StorageFactory()
        {
        }

        private const String Text = "text";
        private const String Local = "local";
        private const String MangoDB = "mangodb";

        public static IStoragePlugin CreateStorage(string storageName)
        {
            var sanitizedStorageName = storageName?.Trim().ToLower();

            switch (sanitizedStorageName)
            {
                case Text:
                    return new TextStorage();

                case Local:
                    return new StorageLocalFile();

                case MangoDB:
                    return new StorageMongoDB();
            }

            /*
             * Fallback if no storage is found
             *
             * you may want to change this.
             */
            return new TextStorage();
        }
    }
}