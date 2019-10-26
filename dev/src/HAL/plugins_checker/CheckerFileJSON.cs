using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace plugins_checker
{
    internal class CheckerFileJSON
    {
        private JObject Root { get; set; }

        public IReadOnlyList<string> PathToCheck => pathToCheck.AsReadOnly();
        private List<string> pathToCheck = new List<string>();

        public void Load(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"'{file} not found.'");
            }

            string jsonData = File.ReadAllText(file);

            // root is composed of all the leaves
            Root = JObject.Parse(jsonData);

            Console.WriteLine($"Configuration file {file} loaded");

            SetFoldersToCheck();
        }

        private void SetFoldersToCheck()
        {
            var foldersPathList = Root["paths"];

            pathToCheck = foldersPathList.ToObject<List<string>>();
        }
    }
}