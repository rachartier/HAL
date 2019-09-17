using System;

namespace dll 
{
    public class Plugin
    {
			public string Run()
			{
				return "dll: " + DateTime.Now.ToString();
			}
    }
}
