using System;

namespace HAL.Plugin
{
    public class PluginSocketInfo : PluginFileInfos
    {

        public PluginSocketInfo(string path, string checksum)
            : base(path)
        {
            FilePath = path;
            CheckSum = checksum;
        }
    }
}
