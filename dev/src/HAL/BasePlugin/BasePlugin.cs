using HAL.OSData;
using System.Linq;
using static HAL.Plugin.PluginFileInfos;

namespace HAL.Plugin
{
    public class BasePlugin : APlugin
    {
        public BasePlugin(IPluginMaster pluginMaster, string path)
        : base(path)
        {
            Type = GetPluginType(pluginMaster);
        }

        public override bool CanBeRunOnOS()
        {
            return ((OsAuthorized & OSAttribute.TargetFlag.Linux) != 0) && OSAttribute.IsLinux
                || ((OsAuthorized & OSAttribute.TargetFlag.Windows) != 0) && OSAttribute.IsWindows
                || ((OsAuthorized & OSAttribute.TargetFlag.OSX) != 0) && OSAttribute.IsOSX;
        }

        public override bool CanBeRun()
        {
            return (Activated && CanBeRunOnOS());
        }

        private FileType GetPluginType(IPluginMaster pluginMaster)
        {
            foreach (var pair in pluginMaster.AcceptedFilesTypes)
            {
                if (pair.Value.Contains(Infos.FileExtension))
                {
                    return pair.Key;
                }
            }

            return FileType.Unknown;
        }
    }
}