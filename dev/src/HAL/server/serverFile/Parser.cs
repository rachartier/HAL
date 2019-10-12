using HAL.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server
{
    public class Parser
    {
        /// <summary>
        /// Parse a receive Data in the format " {path} : {checksum} " with a <EOF>, into a PluginFileInfo
        /// </summary>
        /// <param name="data"></param>
        /// <returns>A PluginSocketInfos with a path and a checksum</returns>
        public static PluginSocketInfo ParseOnePluginFromData(string data)
        {
            string[] splitData = data.Split(' ');
            List<string> pluginData = new List<string>();
            string path;
            string checksum;

            foreach (string word in splitData)
            {
                word.Trim();
                if (word.Equals(":") || word.Contains("<EOF>") || String.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                pluginData.Add(word);
            }

            path = pluginData.ElementAt(0);
            checksum = pluginData.ElementAt(1);

            return new PluginSocketInfo(Path.GetFileName(path), checksum);
        }
    }
}