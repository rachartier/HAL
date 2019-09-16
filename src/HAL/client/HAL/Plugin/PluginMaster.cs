using HAL.Loggin;
using System;
using System.Collections.Generic;

namespace HAL.Plugin
{
    public class PluginMaster
    {
        public enum ReturnCode
        {
            Success,
            Failure
        }

        public IDictionary<BasePlugin.FileType, List<string>> AcceptedFilesTypes = new Dictionary<BasePlugin.FileType, List<string>>()
        {
            [BasePlugin.FileType.DLL] = new List<string> { ".dll" },
            [BasePlugin.FileType.Script] = new List<string> { },
            [BasePlugin.FileType.SharedObject] = new List<string> { ".so" }
        };

        public IDictionary<string, string> ExtensionsNames = new Dictionary<string, string>();
        public IDictionary<string, string> ExtensionToIntepreterName = new Dictionary<string, string>();

        public IReadOnlyList<BasePlugin> Plugins
        {
            get
            {
                return plugins.AsReadOnly();
            }
        }
        private readonly List<BasePlugin> plugins = new List<BasePlugin>();

        public PluginMaster()
        {
            // all officialy supported extensions
            AddScriptExtension(".py", "python");
            AddScriptExtension(".rb", "ruby");
            AddScriptExtension(".pl", "perl");
            AddScriptExtension(".lua", "lua");
            AddScriptExtension(".sh", "bash");
        }

        /// <summary>
        /// add a custom script extension
        /// </summary>
        /// <param name="extension">extension name</param>
        /// <param name="name">complete name</param>
        public void AddScriptExtension(string extension, string name)
        {
            if (AcceptedFilesTypes[BasePlugin.FileType.Script].Contains(extension))
            {
                throw new ArgumentException($"{extension} is already definded.");
            }

            AcceptedFilesTypes[BasePlugin.FileType.Script].Add(extension);

            ExtensionsNames.Add(extension, name);
            Log.Instance.Info($"Extension {name} ({extension}) added.");
        }

        /// <summary>
        /// add a plugin by its path
        /// </summary>
        /// <param name="path">path of the plugin</param>
        public void AddPlugin<TPlugin>(string path) where TPlugin : BasePlugin
        {
            var plugin = (TPlugin)Activator.CreateInstance(typeof(TPlugin), this, path);
            plugins.Add(plugin);

            Log.Instance.Info($"Plugin {path} loaded.");
        }
    }
}
