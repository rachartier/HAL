using System;

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
