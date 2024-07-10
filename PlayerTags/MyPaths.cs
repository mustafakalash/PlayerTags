using System.IO;

namespace PlayerTags;

internal class MyPaths
{
    private static string? _PluginDirectoryPath = null;

    public static string PluginDirectoryPath
    {
        get
        {
            if (_PluginDirectoryPath is null)
            {
                var path = Path.GetDirectoryName(PluginServices.DalamudPluginInterface.AssemblyLocation.FullName);
                if (path is null)
                    _PluginDirectoryPath = string.Empty;
                else
                    _PluginDirectoryPath = path;
            }
            return _PluginDirectoryPath;
        }
    }

    public static string ResourcePath
        => Path.Combine(PluginDirectoryPath, "Resources");
}
