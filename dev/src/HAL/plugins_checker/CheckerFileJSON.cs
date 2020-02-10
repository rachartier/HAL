using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace plugins_checker
{
    internal class CheckerFileJSON
    {
        private List<string> pathToCheck = new List<string>();
        private JObject Root { get; set; }

        public IReadOnlyList<string> PathToCheck => pathToCheck.AsReadOnly();

        public void Load(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException($"'{file} not found.'");

            var jsonData = File.ReadAllText(file);

            // root is composed of all the leaves
            Root = JObject.Parse(jsonData);

            SetFoldersToCheck();
        }

        private void SetFoldersToCheck()
        {
            var foldersPathList = Root["paths"];

            pathToCheck = foldersPathList.ToObject<List<string>>();
        }
    }
}