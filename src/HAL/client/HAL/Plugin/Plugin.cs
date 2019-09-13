using System.IO;
using System.Collections.Generic;
using HAL.OSData;
using System;

namespace HAL.Plugin
{
    public class PluginResultArgs
    {
        public readonly string Result;
        public PluginResultArgs(string result)
        {
            Result = result;
        }
    }

    public class PluginFile
    {
        public delegate void PluginResultHandler(object sender, PluginResultArgs e);

        public event PluginResultHandler OnExecutionFinished;

        public void RaiseOnExecutionFinished(string result)
        {
            OnExecutionFinished?.Invoke(this, new PluginResultArgs(result));
        }

        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string FileExtension { get; private set; }
        public string Name { get; private set; }

        public PluginMaster.FileType Type { get; private set; }

        public OSAttribute.TargetFlag OsAuthorized = 0;
        public double Hearthbeat { get; set; } = 1;
        public bool Activated { get; set; } = false;

        public PluginFile(PluginMaster pluginMaster, string path)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileName(path);
            FileExtension = Path.GetExtension(FileName);
            Type = getPluginType(pluginMaster);
            Name = Path.GetFileNameWithoutExtension(FilePath);
        }

        public bool CanBeRunOnOS()
        {
            return ((((OsAuthorized & OSAttribute.TargetFlag.Linux) != 0) && OSAttribute.IsLinux
                || ((OsAuthorized & OSAttribute.TargetFlag.Windows) != 0) && OSAttribute.IsWindows
                || ((OsAuthorized & OSAttribute.TargetFlag.OSX) != 0) && OSAttribute.IsOSX));
        }

        /// <summary>
        /// verify if the plugin can be run
        /// </summary>
        /// <returns>true if it can be run, false otherwise</returns>
        public bool CanBeRun()
        {
            return (Activated == true && CanBeRunOnOS());
        }

        private PluginMaster.FileType getPluginType(PluginMaster pluginMaster)
        {
            foreach (var pair in pluginMaster.AcceptedFilesTypes)
            {
                foreach (string ext in pair.Value)
                {
                    if (ext.Equals(FileExtension))
                    {
                        return pair.Key;
                    }
                }
            }

            return PluginMaster.FileType.Unknown;
        }
    }
}
