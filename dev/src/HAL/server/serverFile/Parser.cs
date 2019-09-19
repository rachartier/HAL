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
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>A PluginFileInfos with a path and a DateTime of the last writen access</returns>
        public static PluginFileInfos ParseOnePluginFromData(string data)
        {
            string[] splitData = data.Split(' ');
            List<string> pluginData = new List<string>();
            string path;
            string date;
            string time;
            int day, month, year, hour, minute, second;

            foreach (string word in splitData)
            {
                word.Trim();
                if (word.Equals(":") || word.Contains("<EOF>") || String.IsNullOrWhiteSpace(word)) continue;
                pluginData.Add(word);
            }

            path = pluginData.ElementAt(0);
            date = pluginData.ElementAt(1);
            time = pluginData.ElementAt(2);

            day = Int32.Parse(date.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAt(0));
            month = Int32.Parse(date.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAt(1));
            year = Int32.Parse(date.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAt(2));

            hour = Int32.Parse(time.Split(':', StringSplitOptions.RemoveEmptyEntries).ElementAt(0));
            minute = Int32.Parse(time.Split(':', StringSplitOptions.RemoveEmptyEntries).ElementAt(1));
            second = Int32.Parse(time.Split(':', StringSplitOptions.RemoveEmptyEntries).ElementAt(2));

            return new PluginFileInfos(Path.GetFileName(path), new DateTime(year, month, day, hour, minute, second));
        }
    }
}
