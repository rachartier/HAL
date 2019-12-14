using HAL.Storage;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HAL.Factory
{
    public static class StorageFactory
    {
        /*
         * If you want to add a custom storage, you first need to add an attribute wich will defined your custom storage
         * Example:
         *  private const String MyCustomOracle = "oracle"
         *
         *  then you'll be able to specified "oracle" in config.json
         */
        private const string Text = "text";
        private const string Local = "local";
        private const string MangoDB = "mangodb";
        private const string Server = "server";

        public static IStoragePlugin CreateStorage(string storageName)
        {
            var sanitizedStorageName = storageName?.Trim().ToLower();

            /*
             * You also need to add a case here
             */
            return sanitizedStorageName switch
            {
                Text => new StorageText(),
                Local => new StorageLocalFile(),
                MangoDB => new StorageMongoDB(),
                Server => new StorageServerFile(),

                /*
                * Fallback if no storage is found
                *
                * you may want to change this.
                */
                _ => new StorageText(),
            };
        }
    }
}