using HAL.OSData;
using System;
using System.IO;

namespace HAL.Plugin
{
    public class PluginResultArgs : EventArgs
    {
        public readonly string Result;
        public readonly PluginFile Plugin;

        public PluginResultArgs(PluginFile plugin, string result)
        {
            Plugin = plugin;
            Result = result;
        }
    }

    public class PluginFile
    {
        public delegate void PluginResultHandler(object sender, PluginResultArgs e);

        public event PluginResultHandler OnExecutionFinished;

        /// <summary>
        /// this event is raised when the execution of the plugin is completed
        /// </summary>
        /// <param name="result"></param>
        public void RaiseOnExecutionFinished(string result)
        {
            OnExecutionFinished?.Invoke(this, new PluginResultArgs(this, result)); ;
        }

        public readonly string FileName;
        public readonly string FilePath;
        public readonly string FileExtension;
        public readonly string Name;

        public readonly PluginMaster.FileType Type;

        public OSAttribute.TargetFlag OsAuthorized { get; set; } = 0;
        public double Hearthbeat { get; set; } = 1;
        public bool Activated { get; set; } = false;

        public PluginFile(PluginMaster pluginMaster, string path)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileName(path);
            FileExtension = Path.GetExtension(FileName);
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Type = getPluginType(pluginMaster);
        }

        /// <summary>
        /// verify if the plugin can be run on this os
        /// </summary>
        /// <returns></returns>
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
                if (pair.Value.Contains(FileExtension))
                {
                    return pair.Key;
                }
            }

            return PluginMaster.FileType.Unknown;
        }
    }
}
