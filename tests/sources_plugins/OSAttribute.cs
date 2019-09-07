using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


public class OSAttribute
{
    [Flags]
    public enum TargetFlag
    {
        Linux = 1,
        Windows = 2,
        OSX = 4,
        All = Linux | Windows | OSX
    }

    public static bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static Dictionary<TargetFlag, string> TargetOSName = new Dictionary<TargetFlag, string>()
    {
        [TargetFlag.Linux] = "linux",
        [TargetFlag.Windows] = "windows",
        [TargetFlag.OSX] = "osx"
    };

    public static Dictionary<string, OSAttribute.TargetFlag> OSNameToTargetFlag = new Dictionary<string, OSAttribute.TargetFlag>()
    {
        ["linux"] = OSAttribute.TargetFlag.Linux,
        ["windows"] = OSAttribute.TargetFlag.Windows,
        ["osx"] = OSAttribute.TargetFlag.OSX
    };


    public static string GetOSFamillyName()
    {
        string name = "";

        name = IsLinux   ? TargetOSName[TargetFlag.Linux] : name;
        name = IsWindows ? TargetOSName[TargetFlag.Windows] : name;
        name = IsOSX     ? TargetOSName[TargetFlag.OSX] : name;

        return name;
    }
}
