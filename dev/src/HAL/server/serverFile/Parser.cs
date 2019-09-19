using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class Parser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>A Tuple of String that contain the path and a DateTime that contain the date</returns>
        public static (string path, DateTime date) ParseOnePluginFromData(string data)
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

            Console.WriteLine("chemin : {0} date : {1} time : {2}", path, date, time);

            return (path, new DateTime(year, month, day, hour, minute, second));
        }
    }
}
