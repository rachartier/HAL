using HAL.Loggin;
using HAL.Plugin;
using System;

namespace Plugin.Manager
{
    public class PluginMasterBasePlugin : APluginMaster<BasePlugin>
    {
        public PluginMasterBasePlugin()
            : base()
        {
            // all officialy supported extensions
            AddScriptExtension(".py", "python");
            AddScriptExtension(".rb", "ruby");
            AddScriptExtension(".sh", "bash");
            AddScriptExtension(".ps1", "powershell");
        }

        /// <summary>
        /// add a custom script extension
        /// </summary>
        /// <param name="extension">extension name</param>
        /// <param name="name">complete name</param>
        public override void AddScriptExtension(string extension, string name)
        {
            if (AcceptedFilesTypes[PluginFileInfos.FileType.Script].Contains(extension))
            {
                throw new ArgumentException($"{extension} is already definded.");
            }
            
            AcceptedFilesTypes[PluginFileInfos.FileType.Script].Add(extension);

            ExtensionsNames.Add(extension, name);
            Log.Instance?.Info($"Extension {name} ({extension}) added.");
        }

        /// <summary>
        /// add a plugin by its path
        /// </summary>
        /// <param name="path">path of the plugin</param>
        public override void AddPlugin(string path)
        {
            plugins.Add(new BasePlugin(this, path));

            Log.Instance?.Info($"Plugin {path} loaded.");
        }
    }
}