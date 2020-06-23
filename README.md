# SkiaSharp "Extensions" Playground

## Apps

Each of the apps consist of 2 plugins for a container app.

An example would be Visual Studio and 2 extensions, each using different versions of a native library.

```csharp
var appRoot = Path.GetDirectoryName(typeof(Program).Assembly.Location);
var pluginPath = Path.Combine(appRoot, name, "ManagedCore.dll");

var assembly = Assembly.LoadFile(pluginPath);
var type = assembly.GetType("Managed.Thing");

var plugin = (IPlugin)Activator.CreateInstance(type);

Console.WriteLine($"Managed version: {plugin.ManagedVersion}");
Console.WriteLine($"Native version:  {plugin.NativeVersion}");
```

### Core

.NET Core has a nice way to intercept `[DllImport]` loads, so that can (optionall) be used to control which native library is loaded.

```csharp
public class Thing : IPlugin {
    public int ManagedVersion => 1; // or 2 for the v2 plugin
    public int NativeVersion => get_version();

    static Thing() {
        var thisAssembly = typeof(Thing).Assembly;
        // add the hook to intercept the load
        NativeLibrary.SetDllImportResolver(thisAssembly, (libraryName, assembly, searchPath) => {
            if (libraryName != "Native" || assembly != thisAssembly)
                return IntPtr.Zero;
            var assemblyRoot = Path.GetDirectoryName(typeof(Thing).Assembly.Location);
            var libraryPath = Path.Combine(assemblyRoot, "Native.dll");
            // load the library manually
            return UnsafeNativeMethods.LoadLibrary(libraryPath);
        });
    }

    // the usual entrypoint
    [DllImport("Native", CallingConvention = CallingConvention.Cdecl)]
    private static extern int get_version();

    // known, unchanging OS libraries
    private static class UnsafeNativeMethods {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);
    }
}
```

### Full Framework

.NET Framework doesn't have a way to intercept the loading of libraries when using `[DllImport]`, so the managed library has to manually load, look up and create the delegates.

```csharp
public class Thing : IPlugin {
    public int ManagedVersion => 1; // or 2 for the v2 plugin
    public int NativeVersion => Load<get_version_delegate>().Invoke()

    static T Load<T>() {
        var assemblyRoot = Path.GetDirectoryName(typeof(Thing).Assembly.Location);
        var libraryPath = Path.Combine(assemblyRoot, "Native.dll");
        // load the library manually
        var libraryHandle = Kernel32.LoadLibrary(libraryPath);
        // locate the entry point
        var funtionHandle = Kernel32.GetProcAddress(libraryHandle, "get_version");
        // get a managed delegate
        return Marshal.GetDelegateForFunctionPointer<T>(funtionHandle);
    }

    // a delegate representing the entry point
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int get_version_delegate();

    // known, unchanging OS libraries
    static class Kernel32 {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    }
}
```

## Native

A C++ library that just returns its version.

```cpp
// header
extern "C" {
	__declspec(dllexport) int get_version();
}

// source
int get_version() {
	return 1; // or 2 for the v2 plugin
}
```

## Plugins SDK

A simple interface for the plugins, shared by all the projects (app and libraries).

```csharp
public interface IPlugin
{
    int ManagedVersion { get; }
    int NativeVersion { get; }
}
```