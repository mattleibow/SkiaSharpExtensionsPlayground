namespace PluginsSDK
{
	public interface IPlugin
	{
		int ManagedVersion { get; }

		int NativeVersion { get; }
	}
}
