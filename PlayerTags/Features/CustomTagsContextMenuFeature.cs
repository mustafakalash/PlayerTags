using Dalamud.Game.Gui.ContextMenu;
using PlayerTags.Configuration;
using PlayerTags.Data;
using PlayerTags.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerTags.Features;

/// <summary>
/// A feature that adds options for the management of custom tags to context menus.
/// </summary>
public class CustomTagsContextMenuFeature : FeatureBase, IDisposable
{
    private readonly string[] supportedAddonNames =
    [
        null,
        "_PartyList",
        "ChatLog",
        "ContactList",
        "ContentMemberList",
        "CrossWorldLinkshell",
        "FreeCompany",
        "FriendList",
        "LookingForGroup",
        "LinkShell",
        "PartyMemberList",
        "SocialList",
    ];

    public CustomTagsContextMenuFeature(PluginConfiguration pluginConfiguration, PluginData pluginData) : base(pluginConfiguration, pluginData)
    {
        PluginServices.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened; ;
    }

    public void Dispose()
    {
        PluginServices.ContextMenu.OnMenuOpened -= ContextMenu_OnMenuOpened;
    }

    private void ContextMenu_OnMenuOpened(IMenuOpenedArgs args)
    {
        if (!EnableGlobal || !pluginConfiguration.IsCustomTagsContextMenuEnabled
            || args.MenuType != ContextMenuType.Default
            || args.Target is not MenuTargetDefault menuTarget
            || !supportedAddonNames.Contains(args.AddonName))
            return;

        Identity? identity = pluginData.GetIdentity(menuTarget);
        if (identity != null)
        {
            var allTags = new Dictionary<Tag, bool>();
            foreach (var customTag in pluginData.CustomTags)
            {
                var isAdded = identity.CustomTagIds.Contains(customTag.CustomId.Value);
                allTags.Add(customTag, isAdded);
            }

            var sortedTags = allTags.OrderBy(n => n.Value);
            foreach (var tag in sortedTags)
            {
                string menuItemText;
                if (tag.Value)
                    menuItemText = Strings.Loc_Static_ContextMenu_RemoveTag;
                else
                    menuItemText = Strings.Loc_Static_ContextMenu_AddTag;
                menuItemText = string.Format(menuItemText, tag.Key.Text.Value);

                args.AddMenuItem(new()
                {
                    IsSubmenu = false,
                    IsEnabled = true,
                    Name = menuItemText,
                    OnClicked = openedEventArgs =>
                    {
                        if (tag.Value)
                            pluginData.RemoveCustomTagFromIdentity(tag.Key, identity);
                        else
                            pluginData.AddCustomTagToIdentity(tag.Key, identity);
                        pluginConfiguration.Save(pluginData);
                    },
                });
            }
        }
    }
}
