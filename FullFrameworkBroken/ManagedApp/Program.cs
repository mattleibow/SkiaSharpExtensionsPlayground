using PluginsSDK;
using System;
using System.IO;
using System.Reflection;

namespace ManagedApp
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			Console.WriteLine("Loading Visual Studio 2020...");

			Console.WriteLine("Loading plugins...");
			
			var pluginV1 = LoadPlugin("PluginV1");
			var pluginV2 = LoadPlugin("PluginV2");
			
			Console.WriteLine("All plugins loaded.");

			Console.WriteLine("Executing plugin v1...");
			Console.WriteLine($"  Managed version: {pluginV1.ManagedVersion}");
			Console.WriteLine($"  Native version:  {pluginV1.NativeVersion}");

			Console.WriteLine("Executing plugin v2...");
			Console.WriteLine($"  Managed version: {pluginV2.ManagedVersion}");
			Console.WriteLine($"  Native version:  {pluginV2.NativeVersion}");

			Console.WriteLine("Work complete.");

			return 0;
		}

		private static IPlugin LoadPlugin(string name)
		{
			var appRoot = Path.GetDirectoryName(typeof(Program).Assembly.Location);
			var pluginPath = Path.Combine(appRoot, name, "ManagedBroken.dll");

			var assembly = Assembly.LoadFile(pluginPath);
			var type = assembly.GetType("Managed.Thing");

			return (IPlugin)Activator.CreateInstance(type);
		}
	}
}
