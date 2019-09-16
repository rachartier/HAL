using HAL.OSData;
using System;

namespace HAL.Plugin
{
    public class PluginResultArgs : EventArgs
    {
        public readonly string Result;
        public readonly BasePlugin Plugin;

        public PluginResultArgs(BasePlugin plugin, string result)
        {
            Plugin = plugin;
            Result = result;
        }
    }

    public class BasePlugin : PluginFileInfo
    {
        public enum FileType
        {
            Unknown,
            DLL,
            Script,
            SharedObject
        }

        public delegate void PluginResultHandler(object sender, PluginResultArgs e);
        public event PluginResultHandler OnExecutionFinished;

        public readonly FileType Type;

        public OSAttribute.TargetFlag OsAuthorized { get; set; } = 0;
        public double Hearthbeat { get; set; } = 1;
        public bool Activated { get; set; } = false;
        public bool AdministratorRights { get; set; } = false;

        public BasePlugin(PluginMaster pluginMaster, string path)
        : base(path)
        {
            Type = getPluginType(pluginMaster);
        }

        /// <summary>
        /// this event is raised when the execution of the plugin is completed
        /// </summary>
        /// <param name="result">plugin's resultat</param>
        public void RaiseOnExecutionFinished(string result)
        {
            OnExecutionFinished?.Invoke(this, new PluginResultArgs(this, result)); ;
        }

        /// <summary>
        /// verify if the plugin can be run on this os
        /// </summary>
        /// <returns>true if it can be run on this os, false otherwise</returns>
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
            return (Activated && CanBeRunOnOS());
        }

        private FileType getPluginType(PluginMaster pluginMaster)
        {
            foreach (var pair in pluginMaster.AcceptedFilesTypes)
            {
                if (pair.Value.Contains(FileExtension))
                {
                    return pair.Key;
                }
            }

            return FileType.Unknown;
        }
    }
}
