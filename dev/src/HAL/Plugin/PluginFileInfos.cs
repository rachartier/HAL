using System.IO;

namespace HAL.Plugin
{
    public class PluginFileInfos
    {
        public enum FileType
        {
            Unknown,
            DLL,
            Script,
            SharedObject
        }

        public readonly string FileName;
        public readonly string FilePath;
        public readonly string FileExtension;
        public readonly string Name;

        public PluginFileInfos(string path)
        {
            FileName = Path.GetFileName(path);
            FilePath = Path.GetFullPath(path);
            FileExtension = Path.GetExtension(FileName);
            Name = Path.GetFileNameWithoutExtension(FilePath);
        }
    }
}
