using HAL.Loggin;
using System;
using System.Collections.Generic;
using System.Text;

namespace HAL.Plugin
{
    public class PluginMaster
    {
        public enum FileType
        {
            Unknown,
            DLL,
            Script,
            SharedObject
        }

        public enum ReturnCode
        {
            Success,
            Failure
        }

        public IDictionary<FileType, List<string>> AcceptedFilesTypes = new Dictionary<PluginMaster.FileType, List<string>>()
        {
            [FileType.DLL] = new List<string> { ".dll" },
            [FileType.Script] = new List<string> { },
            [FileType.SharedObject] = new List<string> { ".so" }
        };

        public IDictionary<string, string> ExtensionsNames = new Dictionary<string, string>();
        public IDictionary<string, string> ExtensionToIntepreterName = new Dictionary<string, string>();

        public IReadOnlyList<PluginFile> Plugins
        {
            get
            {
                return plugins.AsReadOnly();
            }
        }
        private List<PluginFile> plugins = new List<PluginFile>();

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
            if(AcceptedFilesTypes[FileType.Script].Contains(extension))
            {
                throw new ArgumentException($"{extension} is already definded.");
            }

            AcceptedFilesTypes[FileType.Script].Add(extension);

            ExtensionsNames.Add(extension, name);
            Log.Instance.Info($"Extension {name} ({extension}) added.");
        }

        /// <summary>
        /// add a plugin by its path
        /// </summary>
        /// <param name="path">path of the plugin</param>
        public void AddPlugin(string path)
        {
            plugins.Add(new PluginFile(this, path));

            Log.Instance.Info($"Plugin {path} loaded.");
        }
    }
}
