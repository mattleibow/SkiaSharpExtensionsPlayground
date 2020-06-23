using PluginsSDK;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Managed
{
	public partial class Thing : IPlugin
	{
		static Thing()
		{
			var thisAssembly = typeof(Thing).Assembly;

			NativeLibrary.SetDllImportResolver(thisAssembly, (libraryName, assembly, searchPath) =>
			{
				if (libraryName != "Native" || assembly != thisAssembly)
					return IntPtr.Zero;

				var assemblyRoot = Path.GetDirectoryName(typeof(Thing).Assembly.Location);
				var libraryPath = Path.Combine(assemblyRoot, "Native.dll");

				return UnsafeNativeMethods.LoadLibrary(libraryPath);
			});
		}

		public int NativeVersion => get_version();

		[DllImport("Native", CallingConvention = CallingConvention.Cdecl)]
		private static extern int get_version();

		private static class UnsafeNativeMethods
		{
			[DllImport("Kernel32.dll", SetLastError = true)]
			public static extern IntPtr LoadLibrary(string lpFileName);
		}
	}
}
