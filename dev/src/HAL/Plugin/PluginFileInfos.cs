using System;
using System.IO;

namespace HAL.Plugin
{
    public class PluginFileInfos : IEquatable<PluginFileInfos>
    {
        public enum FileType
        {
            Unknown,
            DLL,
            Script,
            SharedObject
        }

        public string FileName { get; protected set; }
        public string FilePath { get; protected set; }
        public string FileExtension { get; protected set; }
        public string Name { get; protected set; }
        public DateTime DateLastWrite { get; protected set; }

        public PluginFileInfos(string path)
        {
            FileName = Path.GetFileName(path);
            FilePath = Path.GetFullPath(path);
            FileExtension = Path.GetExtension(FileName);
            Name = Path.GetFileNameWithoutExtension(FilePath);
            DateLastWrite = File.GetLastWriteTime(FilePath);
        }

        public bool Equals(PluginFileInfos other)
        {
            if (this == other)
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return FileName.Equals(other.FileName);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(FileName);
        }
    }
}
