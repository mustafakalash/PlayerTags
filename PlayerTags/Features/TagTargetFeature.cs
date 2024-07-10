using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Pilz.Dalamud.ActivityContexts;
using Pilz.Dalamud.Tools.Strings;
using PlayerTags.Configuration;
using PlayerTags.Configuration.GameConfig;
using PlayerTags.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerTags.Features;

/// <summary>
/// The base of a feature that adds tags to UI elements.
/// </summary>
public abstract class TagTargetFeature(PluginConfiguration pluginConfiguration, PluginData pluginData) : FeatureBase(pluginConfiguration, pluginData), IDisposable
{
    public ActivityContextManager ActivityContextManager { get; init; } = new();

    public virtual void Dispose()
    {
        ActivityContextManager.Dispose();
        GC.SuppressFinalize(this);
    }

    protected abstract bool IsIconVisible(Tag tag);

    protected abstract bool IsTextVisible(Tag tag);

    protected bool IsTagVisible(Tag tag, IGameObject? gameObject)
    {
        bool isVisibleForActivity = ActivityContextHelper.GetIsVisible(ActivityContextManager.CurrentActivityContext.ActivityType,
            tag.IsVisibleInPveDuties.InheritedValue ?? false,
            tag.IsVisibleInPvpDuties.InheritedValue ?? false,
            tag.IsVisibleInOverworld.InheritedValue ?? false);

        if (!isVisibleForActivity)
            return false;

        if (gameObject is IPlayerCharacter playerCharacter)
        {
            bool isVisibleForPlayer = PlayerContextHelper.GetIsVisible(playerCharacter,
                tag.IsVisibleForSelf.InheritedValue ?? false,
                tag.IsVisibleForFriendPlayers.InheritedValue ?? false,
                tag.IsVisibleForPartyPlayers.InheritedValue ?? false,
                tag.IsVisibleForAlliancePlayers.InheritedValue ?? false,
                tag.IsVisibleForEnemyPlayers.InheritedValue ?? false,
                tag.IsVisibleForOtherPlayers.InheritedValue ?? false);

            if (!isVisibleForPlayer)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the payloads for the given tag and game object depending on visibility conditions.
    /// </summary>
    /// <param name="gameObject">The game object to get payloads for.</param>
    /// <param name="tag">The tag config to get payloads for.</param>
    /// <returns>A list of payloads for the given tag.</returns>
    protected Payload[] GetPayloads(Tag tag, IGameObject? playerCharacter)
    {
        if (!IsTagVisible(tag, playerCharacter))
            return [];
        return CreatePayloads(tag);
    }

    /// <summary>
    /// Creates payloads for the given tag.
    /// </summary>
    /// <param name="tag">The tag to create payloads for.</param>
    /// <returns>The payloads for the given tag.</returns>
    private Payload[] CreatePayloads(Tag tag)
    {
        List<Payload> newPayloads = [];
        BitmapFontIcon? icon = null;
        string? text = null;

        if (IsIconVisible(tag))
            icon = tag.Icon.InheritedValue;

        if (icon != null && icon.Value != BitmapFontIcon.None)
            newPayloads.Add(new IconPayload(icon.Value));

        if (IsTextVisible(tag))
            text = tag.Text.InheritedValue;

        if (!string.IsNullOrWhiteSpace(text))
        {
            if (tag.IsTextItalic.InheritedValue != null && tag.IsTextItalic.InheritedValue.Value)
                newPayloads.Add(new EmphasisItalicPayload(true));

            if (tag.TextGlowColor.InheritedValue != null)
                newPayloads.Add(new UIGlowPayload(tag.TextGlowColor.InheritedValue.Value));

            if (tag.TextColor.InheritedValue != null)
                newPayloads.Add(new UIForegroundPayload(tag.TextColor.InheritedValue.Value));

            newPayloads.Add(new TextPayload(text));

            if (tag.TextColor.InheritedValue != null)
                newPayloads.Add(new UIForegroundPayload(0));

            if (tag.TextGlowColor.InheritedValue != null)
                newPayloads.Add(new UIGlowPayload(0));

            if (tag.IsTextItalic.InheritedValue != null && tag.IsTextItalic.InheritedValue.Value)
                newPayloads.Add(new EmphasisItalicPayload(false));
        }

        return [.. newPayloads];
    }

    protected static string BuildPlayername(string name)
    {
        var logNameType = GameConfigHelper.Instance.GetLogNameType();
        var result = string.Empty;

        if (logNameType != null && !string.IsNullOrEmpty(name))
        {
            var nameSplitted = name.Split(' ');

            if (nameSplitted.Length > 1)
            {
                var firstName = nameSplitted[0];
                var lastName = nameSplitted[1];

                switch (logNameType)
                {
                    case LogNameType.FullName:
                        result = $"{firstName} {lastName}";
                        break;
                    case LogNameType.LastNameShorted:
                        result = $"{firstName} {lastName[..1]}.";
                        break;
                    case LogNameType.FirstNameShorted:
                        result = $"{firstName[..1]}. {lastName}";
                        break;
                    case LogNameType.Initials:
                        result = $"{firstName[..1]}. {lastName[..1]}.";
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(result))
            result = name;

        return result;
    }

    /// <summary>
    /// Adds the given payload changes to the dictionary.
    /// </summary>
    /// <param name="tagPosition">The position to add changes to.</param>
    /// <param name="payloads">The payloads to add.</param>
    /// <param name="stringChanges">The dictionary to add the changes to.</param>
    protected void AddPayloadChanges(StringPosition tagPosition, IEnumerable<Payload> payloads, StringChanges stringChanges, bool forceUsingSingleAnchorPayload)
    {
        if (payloads != null && payloads.Any() && stringChanges != null)
        {
            var changes = stringChanges.GetChange(tagPosition);
            changes.Payloads.AddRange(payloads);
            changes.ForceUsingSingleAnchorPayload = forceUsingSingleAnchorPayload;
        }
    }
}
