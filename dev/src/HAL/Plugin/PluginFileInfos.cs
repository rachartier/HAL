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

        public bool Equals(PluginFileInfos other)
        {
            if (other is null)
            {
                return false;
            }

            if (this == other)
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return FileName.Equals(other.FileName)
                && FilePath.Equals(other.FilePath)
                && FileExtension.Equals(other.FileExtension)
                && Name.Equals(other.Name);
        }
    }
}