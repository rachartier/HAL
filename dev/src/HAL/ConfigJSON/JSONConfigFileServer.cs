using System;
using HAL.Loggin;
using HAL.MagicString;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HAL.Configuration
{
    public class JSONConfigFileServer : IConfigFileServer<JObject, JToken>
    {
        public static int DEFAULT_PORT = 11000;
        public static int DEFAULT_UPDATE_RATE = 10000;

        public JSONConfigFileServer()
        {
        }

        public override void Load(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"'{file} not found.'");
            }

            string jsonData = File.ReadAllText(file);

            // root is composed of all the leaves
            Root = JObject.Parse(jsonData);

            Log.Instance?.Info($"Configuration file {file} loaded");
        }

        public override int GetPort()
        {
            if (Root == null)
            {
                return DEFAULT_PORT;
            }

            int? port = Root.Value<int?>(MagicStringEnumerator.JSONPort);

            if(port == null)
                return DEFAULT_PORT;

            return port.Value;
        }

        public override int GetUpdateRate()
        {
            if (Root == null)
            {
                return DEFAULT_UPDATE_RATE;
            }

            int? updateRate = Root.Value<int?>(MagicStringEnumerator.JSONUpdateRate);

            if(updateRate == null)
                return DEFAULT_UPDATE_RATE;

            return updateRate.Value;
        }

        public override string GetAddress()
        {
            if (Root == null)
            {
                return null;
            }

            string address = Root.Value<string>(MagicStringEnumerator.JSONAddress);

            return address;
        }

        public override int GetMaxThreads()
        {
            if (Root == null)
            {
                return Environment.ProcessorCount;
            }

            int? maxThreads = Root.Value<int?>(MagicStringEnumerator.JSONMaxThreads);

            if(maxThreads == null)
            {
                return Environment.ProcessorCount;
            }

            return maxThreads.Value;
        }
    }
}