using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PlayerTags;

public class PluginServices
{
    [PluginService] public static IDalamudPluginInterface DalamudPluginInterface { get; set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; set; } = null;
    [PluginService] public static IGameConfig GameConfig { get; set; } = null;
    [PluginService] public static IChatGui ChatGui { get; set; } = null!;
    [PluginService] public static IClientState ClientState { get; set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] public static IDataManager DataManager { get; set; } = null!;
    [PluginService] public static IFramework Framework { get; set; } = null!;
    [PluginService] public static IGameGui GameGui { get; set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; set; } = null!;
    [PluginService] public static IPartyList PartyList { get; set; } = null!;
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; set; } = null;

    public static void Initialize(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PluginServices>();
    }
}
