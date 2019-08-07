﻿using System;
using System.Collections.Generic;

namespace projet_licence
{
    class Program
    {
        static void Main(string[] args)
        {
					List<Plugin> plugins = new List<Plugin>()
					{
						new Plugin("test/dll.dll"),
						new Plugin("test/script.py"),
						new Plugin("test/script.rb"),
						new Plugin("test/script.sh")
					};

					foreach(var plugin in plugins)
					{
						Console.WriteLine($"[{plugin.FileName}, {plugin.Type}]");
						Console.WriteLine(PluginManager.Run(plugin) + "\n");
					}
        }
    }
}
