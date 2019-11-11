using HAL.Loggin;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HAL.Configuration
{
    public class JSONConfigFileServer : IConfigFileServer<JObject, JToken>
    {
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
    }
}