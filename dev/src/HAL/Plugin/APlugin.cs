using HAL.OSData;
using System;

namespace HAL.Plugin
{
    public abstract class APlugin
    {
        public class PluginResultArgs : EventArgs
        {
            public readonly string Result;
            public readonly APlugin Plugin;

            public PluginResultArgs(APlugin plugin, string result)
            {
                Plugin = plugin;
                Result = result;
            }
        }

        public PluginFileInfos Infos { get; private set; }

        /// <summary>
        /// this event is raised when the execution of the plugin is completed
        /// </summary>
        /// <param name="result">plugin's resultat</param>

        public delegate void PluginResultHandler(object sender, PluginResultArgs e);
        public event PluginResultHandler OnExecutionFinished;

        public PluginFileInfos.FileType Type { get; protected set; }
        public OSAttribute.TargetFlag OsAuthorized { get; set; } = 0;

        public double Hearthbeat { get; set; } = 1;
        public bool AdministratorRights { get; set; } = false;
        public bool Activated { get; set; } = false;

        public void RaiseOnExecutionFinished(string result)
        {
            OnExecutionFinished?.Invoke(this, new PluginResultArgs(this, result)); ;
        }

        protected APlugin(string path)
        {
            Infos = new PluginFileInfos(path);
        }

        /// <summary>
        /// verify if the plugin can be run on this os
        /// </summary>
        /// <returns>true if it can be run on this os, false otherwise</returns>
        public abstract bool CanBeRunOnOS();

        /// <summary>
        /// verify if the plugin can be run
        /// </summary>
        /// <returns>true if it can be run, false otherwise</returns>
        public abstract bool CanBeRun();
    }
}
