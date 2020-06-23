using PluginsSDK;
using System.Runtime.InteropServices;

namespace Managed
{
	public partial class Thing : IPlugin
	{
		public int NativeVersion => get_version();

		[DllImport("Native", CallingConvention = CallingConvention.Cdecl)]
		private static extern int get_version();
	}
}
