using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HAL.OSData
{
    public class OSAttribute
    {
        /*
         * Supported Familly OS type
         */
        [Flags]
        public enum TargetFlag
        {
            Linux = 0x00000001,
            Windows = 0x00000002,
            OSX = 0x00000004,
            All = Linux | Windows | OSX
        }

        public static string FamillyLinuxName = "linux";
        public static string FamillyWindowsName = "windows";
        public static string FamillyOSXName = "osx";

        public static bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static IDictionary<TargetFlag, string> TargetOSName = new Dictionary<TargetFlag, string>()
        {
            [TargetFlag.Linux] = FamillyLinuxName,
            [TargetFlag.Windows] = FamillyWindowsName,
            [TargetFlag.OSX] = FamillyOSXName
        };

        public static IDictionary<string, OSAttribute.TargetFlag> OSNameToTargetFlag = new Dictionary<string, OSAttribute.TargetFlag>()
        {
            [FamillyLinuxName] = OSAttribute.TargetFlag.Linux,
            [FamillyWindowsName] = OSAttribute.TargetFlag.Windows,
            [FamillyOSXName] = OSAttribute.TargetFlag.OSX
        };

        /// <summary>
        /// get the running os familly name 
        /// it can be: windows, linux or osx
        /// </summary>
        /// <returns>the os name</returns>
        public static string GetOSFamillyName()
        {
            string name = "";

            name = IsLinux ? TargetOSName[TargetFlag.Linux] : name;
            name = IsWindows ? TargetOSName[TargetFlag.Windows] : name;
            name = IsOSX ? TargetOSName[TargetFlag.OSX] : name;

            return name;
        }
    }
}
