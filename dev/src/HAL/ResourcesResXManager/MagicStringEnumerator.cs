using System;

namespace HAL.MagicString
{
    public static class MagicStringEnumerator
    {
        public static readonly string CMDAdd = "ADD";
        public static readonly string CMDUpd = "UPD"; /* Update */
        public static readonly string CMDDel = "DEL";
        public static readonly string CMDEnd = "END";
        public static readonly string CMDSve = "SVE"; /* Save */
        public static readonly string CMDHtb = "HTB"; /* Heartbeat */

        public static readonly string DefaultPluginPath = AppDomain.CurrentDomain.BaseDirectory + "plugins";
        public static readonly string DefaultNLogConfigPath = AppDomain.CurrentDomain.BaseDirectory + "nlog.config";
        public static readonly string DefaultConfigPath = AppDomain.CurrentDomain.BaseDirectory + "config/config.json";
        public static readonly string DefaultLocalConfigPath = AppDomain.CurrentDomain.BaseDirectory + "config/config_local.json";
        public static readonly string DefaultConfigPathServerToClient = AppDomain.CurrentDomain.BaseDirectory + "plugins/config.json";

        public static readonly string JSONPlugins = "plugins";
        public static readonly string JSONDifferencial = "differencial";
        public static readonly string JSONDifferencialAll = "differencial_all";
        public static readonly string JSONHeartbeat = "heartbeat";
        public static readonly string JSONIntepreter = "interpreter";
        public static readonly string JSONAdminUsername = "admin_username";
        public static readonly string JSONAdminRights = "admin_rights";
        public static readonly string JSONActivated = "activated";
        public static readonly string JSONCustomExtensions = "custom_extensions";
        public static readonly string JSONOs = "os";
        public static readonly string JSONDatabase = "database";
        public static readonly string JSONConnectionString = "connectionString";
        public static readonly string JSONStorageName = "storage";
        public static readonly string JSONServer = "server";

        /** Server **/
        public static readonly string JSONPort = "port";
        public static readonly string JSONMaxThreads = "max_threads";
        public static readonly string JSONAddress = "ip";
        public static readonly string JSONUpdateRate = "update_rate";

        public static readonly string RootSaveResults = "results";

        public static readonly string JSONSavePath = "save_path";
    }
}
