using Microsoft.Extensions.Configuration;

namespace apirest
{
    public class ConfigurationOptions
    {
        public static bool? UseWebSockets => Configuration["UseWebSockets"]?.ToLower().Equals("true");

        public static IConfiguration Configuration { get; set; }
    }
}