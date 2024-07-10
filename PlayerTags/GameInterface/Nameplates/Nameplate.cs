using Pilz.Dalamud.Nameplates;
using System;

namespace PlayerTags.GameInterface.Nameplates;

/// <summary>
/// Provides an interface to modify nameplates.
/// </summary>
public class Nameplate : IDisposable
{
    public NameplateManager NameplateManager { get; init; }

    /// <summary>
    /// Occurs when a player nameplate is updated by the game.
    /// </summary>
    public event PlayerNameplateUpdatedDelegate? PlayerNameplateUpdated;

    /// <summary>
    /// Whether the required hooks are in place and this instance is valid.
    /// </summary>
    public bool IsValid
    {
        get => NameplateManager != null && NameplateManager.IsValid;
    }

    public Nameplate()
    {
        NameplateManager = new();
        NameplateManager.Hooks.AddonNamePlate_SetPlayerNameManaged += Hooks_AddonNamePlate_SetPlayerNameManaged;
    }

    public void Dispose()
    {
        NameplateManager.Hooks.AddonNamePlate_SetPlayerNameManaged -= Hooks_AddonNamePlate_SetPlayerNameManaged;
        NameplateManager.Dispose();
    }

    private void Hooks_AddonNamePlate_SetPlayerNameManaged(Pilz.Dalamud.Nameplates.EventArgs.AddonNamePlate_SetPlayerNameManagedEventArgs eventArgs)
    {
        try
        {
            PlayerCharacter? playerCharacter = NameplateManager.GetNameplateGameObject<PlayerCharacter>(eventArgs.SafeNameplateObject);

            if (playerCharacter != null)
            {
                var playerNameplateUpdatedArgs = new PlayerNameplateUpdatedArgs(playerCharacter, eventArgs);
                PlayerNameplateUpdated?.Invoke(playerNameplateUpdatedArgs);
            }
        }
        catch (Exception ex)
        {
            PluginServices.PluginLog.Error(ex, $"SetPlayerNameplateDetour");
        }
    }
}
