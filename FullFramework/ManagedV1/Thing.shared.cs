using PluginsSDK;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Managed
{
	public partial class Thing : IPlugin
	{
		private static IntPtr libraryHandle;
		private static IntPtr funtionHandle;

		private get_version_delegate get_version;

		public int NativeVersion
		{
			get
			{
				if (get_version == null)
					get_version = Load<get_version_delegate>();
				return get_version.Invoke();
			}
		}

		private static T Load<T>()
		{
			var assemblyRoot = Path.GetDirectoryName(typeof(Thing).Assembly.Location);
			var libraryPath = Path.Combine(assemblyRoot, "Native.dll");

			if (libraryHandle == IntPtr.Zero)
				libraryHandle = UnsafeNativeMethods.LoadLibrary(libraryPath);

			var error = Marshal.GetLastWin32Error();
			if (error != 0)
				throw new Exception($"Error code 0x{error:X8}");

			if (funtionHandle == IntPtr.Zero)
				funtionHandle = UnsafeNativeMethods.GetProcAddress(libraryHandle, "get_version");

			error = Marshal.GetLastWin32Error();
			if (error != 0)
				throw new Exception($"Error code 0x{error:X8}");

			return Marshal.GetDelegateForFunctionPointer<T>(funtionHandle);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int get_version_delegate();

		private static class UnsafeNativeMethods
		{
			[DllImport("Kernel32.dll", SetLastError = true)]
			public static extern IntPtr LoadLibrary(string lpFileName);

			[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
		}
	}
}
