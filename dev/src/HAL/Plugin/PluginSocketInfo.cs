using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HAL.Plugin
{
    public class PluginSocketInfo : PluginFileInfos
    {

        public PluginSocketInfo(string path, DateTime date)
            : base(path)
        {
            FilePath = path;
            DateLastWrite = date;
        }
    }
}
