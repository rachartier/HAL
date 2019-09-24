using System.Collections.Generic;

namespace HAL.Plugin
{
    public interface IPluginMaster
    {
        IDictionary<PluginFileInfos.FileType, List<string>> AcceptedFilesTypes { get; set; }
        IDictionary<string, string> ExtensionsNames { get; set; }
        IReadOnlyList<APlugin> Plugins { get; }

        void AddScriptExtension(string extension, string name);
        void AddPlugin(string path);
    }
}