using System.IO;

namespace HAL.Plugin
{
    public class PluginFileInfo
    {
        public readonly string FileName;
        public readonly string FilePath;
        public readonly string FileExtension;
        public readonly string Name;

        public PluginFileInfo(string path)
        {
            FileName = Path.GetFileName(path);
            FilePath = Path.GetFullPath(path);
            FileExtension = Path.GetExtension(FileName);
            Name = Path.GetFileNameWithoutExtension(FilePath);
        }
    }
}
