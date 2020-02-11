using System;
using System.IO;
using HAL.MagicString;
using Newtonsoft.Json.Linq;

namespace HAL.Configuration
{
    public class JSONConfigFileServer : IConfigFileServer<JObject, JToken>
    {
        private static int DEFAULT_PORT = 11000;
        private static int DEFAULT_UPDATE_RATE = 10000;

        public override void Load(string file) {
            if (!File.Exists(file)) throw new FileNotFoundException($"'{file} not found.'");

            var jsonData = File.ReadAllText(file);

            // root is composed of all the leaves
            Root = JObject.Parse(jsonData);
        }

        public override int GetPort()
        {
            if (Root == null) return DEFAULT_PORT;

            var port = Root.Value<int?>(MagicStringEnumerator.JSONPort);

            if (port == null)
                return DEFAULT_PORT;

            return port.Value;
        }

        public override int GetUpdateRate()
        {
            if (Root == null) return DEFAULT_UPDATE_RATE;

            var updateRate = Root.Value<int?>(MagicStringEnumerator.JSONUpdateRate);

            if (updateRate == null)
                return DEFAULT_UPDATE_RATE;

            return updateRate.Value;
        }

        public override string GetAddress()
        {
            if (Root == null) return null;

            var address = Root.Value<string>(MagicStringEnumerator.JSONAddress);

            return address;
        }

        public override int GetMaxThreads()
        {
            if (Root == null) return Environment.ProcessorCount;

            var maxThreads = Root.Value<int?>(MagicStringEnumerator.JSONMaxThreads);

            if (maxThreads == null) return Environment.ProcessorCount;

            return maxThreads.Value;
        }

        public override string GetSavePath()
        {
            if (Root == null) return null;

            var path = Root.Value<string>(MagicStringEnumerator.JSONSavePath);

            if (string.IsNullOrEmpty(path)) return "results/";

            return path;
        }
    }
}