using HAL.Loggin;
using HAL.MagicString;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HAL.Configuration
{
    public class JSONConfigFileServer : IConfigFileServer<JObject, JToken>
    {
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

        public override int GetMaxConnection()
        {
            if (Root == null)
            {
                return 100;
            }

            int maxConnection = int.Parse(Root.Value<string>(MagicStringEnumerator.JSONMaxConnection));

            return maxConnection;

        }

        public override string GetPath()
        {
            if (Root == null)
            {
                return null;
            }

            string path = Root.Value<string>(MagicStringEnumerator.JSONPath);

            return path;
        }

        public override string GetPluginDirectory()
        {
            if (Root == null)
            {
                return null;
            }

            string dirName = Root.Value<string>(MagicStringEnumerator.JSONPluginDirectory);

            return dirName;
        }

        public override int GetPort()
        {
            if (Root == null)
            {
                return 11000;
            }

            int port = int.Parse(Root.Value<string>(MagicStringEnumerator.JSONPort));

            return port;
        }

        public override int GetRetryMax()
        {
            if (Root == null)
            {
                return 3;
            }

            int retry = int.Parse(Root.Value<string>(MagicStringEnumerator.JSONRetryMax));

            return retry;
        }




    }
}