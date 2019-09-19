using HAL.Loggin;
using System;
using System.Collections.Generic;

namespace HAL.Plugin
{
    public abstract class APluginMaster<TPlugin> : IPluginMaster
        where TPlugin : APlugin
    {
        public enum ReturnCode
        {
            Success,
            Failure
        }

        public IDictionary<PluginFileInfos.FileType, List<string>> AcceptedFilesTypes { get; set; } = new Dictionary<PluginFileInfos.FileType, List<string>>()
        {
            [PluginFileInfos.FileType.DLL] = new List<string> { ".dll" },
            [PluginFileInfos.FileType.Script] = new List<string> { },
            [PluginFileInfos.FileType.SharedObject] = new List<string> { ".so" }
        };

        public IDictionary<string, string> ExtensionsNames { get; set; } = new Dictionary<string, string>();

        public IReadOnlyList<APlugin> Plugins => plugins.AsReadOnly();

        protected readonly List<TPlugin> plugins = new List<TPlugin>();

        /// <summary>
        /// add a custom script extension
        /// </summary>
        /// <param name="extension">extension name</param>
        /// <param name="name">complete name</param>
        public abstract void AddScriptExtension(string extension, string name);

        /// <summary>
        /// add a plugin by its path
        /// </summary>
        /// <param name="path">path of the plugin</param>
        public abstract void AddPlugin(string path);
    }
}
