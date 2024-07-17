using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Pilz.Dalamud.NamePlate;

namespace PlayerTags;

public class PluginServices
{
    [PluginService] public static IDalamudPluginInterface DalamudPluginInterface { get; set; }
    [PluginService] public static IPluginLog PluginLog { get; set; }
    [PluginService] public static IGameConfig GameConfig { get; set; }
    [PluginService] public static IChatGui ChatGui { get; set; }
    [PluginService] public static IClientState ClientState { get; set; }
    [PluginService] public static ICommandManager CommandManager { get; set; }
    [PluginService] public static IDataManager DataManager { get; set; }
    [PluginService] public static IFramework Framework { get; set; }
    [PluginService] public static IGameGui GameGui { get; set; }
    [PluginService] public static IObjectTable ObjectTable { get; set; }
    [PluginService] public static IPartyList PartyList { get; set; }
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; set; }
    [PluginService] public static IContextMenu ContextMenu { get; set; }
    [PluginService] public static INamePlateGui NamePlateGui => INamePlateGui.Instance;

    public static void Initialize(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PluginServices>();
    }
}
