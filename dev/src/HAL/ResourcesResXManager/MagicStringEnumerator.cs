namespace HAL.MagicString
{
    public static class MagicStringEnumerator
    {
        public static readonly string CMDAdd = "ADD";
        public static readonly string CMDUpd = "UPD";
        public static readonly string CMDDel = "DEL";
        public static readonly string CMDEnd = "END";

        public static readonly string DefaultPluginPath = "plugins";
        public static readonly string DefaultConfigPath = "config/config.json";
        public static readonly string DefaultLocalConfigPath = "config/config_local.json";
        public static readonly string DefaultConfigPathServerToClient = "plugins/config.json";

        public static readonly string JSONPlugins = "plugins";
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
    }
}