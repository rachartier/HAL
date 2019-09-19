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

        public readonly string FileName;
        public readonly string FilePath;
        public readonly string FileExtension;
        public readonly string Name;
        public readonly DateTime DateLastWrite;

        public PluginFileInfos(string path)
        {
            FileName = Path.GetFileName(path);
            FilePath = Path.GetFullPath(path);
            FileExtension = Path.GetExtension(FileName);
            Name = Path.GetFileNameWithoutExtension(FilePath);
            DateLastWrite = File.GetLastWriteTime(FilePath);
        }

        public PluginFileInfos(string path, DateTime date)
        {
            FilePath = path;
            DateLastWrite = date;
        }

        public bool Equals(PluginFileInfos other)
        {
            if (this == other) return true;
            if (other == null) return false;
            return this.FileName.Equals(other.FileName);
        }
    }
}
