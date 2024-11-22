using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.Sheets;
using Pilz.Dalamud;
using Pilz.Dalamud.Icons;
using Pilz.Dalamud.Tools.NamePlates;
using Pilz.Dalamud.Tools.Strings;
using PlayerTags.Configuration;
using PlayerTags.Data;
using PlayerTags.Inheritables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerTags.Features;

/// <summary>
/// A feature that adds tags to nameplates.
/// </summary>
public class NameplateTagTargetFeature : TagTargetFeature
{
    private readonly StatusIconPriorizer statusiconPriorizer;
    private readonly JobIconSets jobIconSets = new();

    public NameplateTagTargetFeature(PluginConfiguration pluginConfiguration, PluginData pluginData) : base(pluginConfiguration, pluginData)
    {
        statusiconPriorizer = new(pluginConfiguration.StatusIconPriorizerSettings);
        PluginServices.NamePlateGui.OnNamePlateUpdate += NamePlateGui_OnNamePlateUpdate;
    }

    public override void Dispose()
    {
        PluginServices.NamePlateGui.OnNamePlateUpdate -= NamePlateGui_OnNamePlateUpdate;
        base.Dispose();
    }

    protected override bool IsIconVisible(Tag tag)
    {
        if (tag.IsRoleIconVisibleInNameplates.InheritedValue != null)
            return tag.IsRoleIconVisibleInNameplates.InheritedValue.Value;
        return false;
    }

    protected override bool IsTextVisible(Tag tag)
    {
        if (tag.IsTextVisibleInNameplates.InheritedValue != null)
            return tag.IsTextVisibleInNameplates.InheritedValue.Value;
        return false;
    }

    private void NamePlateGui_OnNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        if (!EnableGlobal)
            return;

        foreach (var handler in handlers)
        {
            // Only handle player nameplates
            if (handler.NamePlateKind != NamePlateKind.PlayerCharacter || handler.PlayerCharacter == null)
                continue;

            var beforeTitleBytes = handler.InfoView.Title.Encode();
            var generalOptions = pluginConfiguration.GeneralOptions[ActivityContextManager.CurrentActivityContext.ActivityType];

            AddTagsToNameplate(handler, generalOptions);

            if (generalOptions.NameplateTitlePosition == NameplateTitlePosition.AlwaysAboveName)
                handler.IsPrefixTitle = true;
            else if (generalOptions.NameplateTitlePosition == NameplateTitlePosition.AlwaysBelowName)
                handler.IsPrefixTitle = false;

            if (generalOptions.NameplateTitleVisibility == NameplateTitleVisibility.Always)
                handler.DisplayTitle = true;
            else if (generalOptions.NameplateTitleVisibility == NameplateTitleVisibility.Never)
                handler.DisplayTitle = false;
            else if (generalOptions.NameplateTitleVisibility == NameplateTitleVisibility.WhenHasTags)
                handler.DisplayTitle = !beforeTitleBytes.SequenceEqual(handler.InfoView.Title.Encode());

            if (generalOptions.NameplateFreeCompanyVisibility == NameplateFreeCompanyVisibility.Never)
                handler.RemoveFreeCompanyTag();
        }
    }

    /// <summary>
    /// Adds the given payload changes to the specified locations.
    /// </summary>
    /// <param name="nameplateElement">The nameplate element of the changes.</param>
    /// <param name="tagPosition">The position of the changes.</param>
    /// <param name="payloadChanges">The payload changes to add.</param>
    /// <param name="nameplateChanges">The dictionary to add changes to.</param>
    private void AddPayloadChanges(NameplateElement nameplateElement, TagPosition tagPosition, IEnumerable<Payload> payloadChanges, NameplateChanges nameplateChanges, bool forceUsingSingleAnchorPayload)
    {
        if (payloadChanges.Any())
        {
            var changes = nameplateChanges.GetChange((NameplateElements)nameplateElement);
            AddPayloadChanges((StringPosition)tagPosition, payloadChanges, changes.Changes, forceUsingSingleAnchorPayload);
        }
    }


    /// <summary>
    /// Adds tags to the nameplate of a game object.
    /// </summary>
    /// <param name="playerCharacter">The game object context.</param>
    /// <param name="name">The name text to change.</param>
    /// <param name="title">The title text to change.</param>
    /// <param name="freeCompany">The free company text to change.</param>
    private void AddTagsToNameplate(INamePlateUpdateHandler handler, GeneralOptionsClass generalOptions)
    {
        int? newStatusIcon = null;
        var nameplateChanges = new NameplateChanges(handler);

        if (handler.PlayerCharacter != null && (!handler.PlayerCharacter.IsDead || generalOptions.NameplateDeadPlayerHandling != DeadPlayerHandling.Ignore))
        {
            var classJob = handler.PlayerCharacter.ClassJob.ValueNullable;

            // Add the job tags
            if (classJob.HasValue && pluginData.JobTags.TryGetValue(classJob.Value.Abbreviation.ParseString(), out var jobTag))
            {
                if (jobTag.TagTargetInNameplates.InheritedValue != null && jobTag.TagPositionInNameplates.InheritedValue != null)
                    checkTag(jobTag);
            }

            // Add the randomly generated name tag payload
            if (pluginConfiguration.IsPlayerNameRandomlyGenerated)
            {
                var characterName = handler.PlayerCharacter.Name.TextValue;
                if (characterName != null)
                {
                    var generatedName = RandomNameGenerator.Generate(characterName);
                    if (generatedName != null)
                        AddPayloadChanges(NameplateElement.Name, TagPosition.Replace, Enumerable.Empty<Payload>().Append(new TextPayload(generatedName)), nameplateChanges, false);
                }
            }

            // Add custom tags
            Identity identity = pluginData.GetIdentity(handler.PlayerCharacter);
            foreach (var customTagId in identity.CustomTagIds)
            {
                var customTag = pluginData.CustomTags.FirstOrDefault(tag => tag.CustomId.Value == customTagId);
                if (customTag != null)
                    checkTag(customTag);
            }

            void checkTag(Tag tag)
            {
                if (tag.TagTargetInNameplates.InheritedValue != null && tag.TagPositionInNameplates.InheritedValue != null)
                {
                    var payloads = GetPayloads(tag, handler.PlayerCharacter);
                    if (payloads.Length != 0)
                        AddPayloadChanges(tag.TagTargetInNameplates.InheritedValue.Value, tag.TagPositionInNameplates.InheritedValue.Value, payloads, nameplateChanges, false);
                }
                if (IsTagVisible(tag, handler.PlayerCharacter) && newStatusIcon == null && classJob != null && (tag.IsJobIconVisibleInNameplates?.InheritedValue ?? false))
                    newStatusIcon = jobIconSets.GetJobIcon(tag.JobIconSet?.InheritedValue ?? JobIconSetName.Framed, classJob.Value.RowId);
            }
        }

        // Apply new status icon
        if (newStatusIcon != null)
        {
            NameplateUpdateFactory.ApplyStatusIconWithPrio(handler, (int)newStatusIcon, ActivityContextManager.CurrentActivityContext, statusiconPriorizer, pluginConfiguration.MoveStatusIconToNameplateTextIfPossible);
        }

        // Build the final strings out of the payloads
        NameplateUpdateFactory.ApplyNameplateChanges(new NameplateChangesProps
        {
            Changes = nameplateChanges
        });

        // Gray out the nameplate
        if (handler.PlayerCharacter != null && handler.PlayerCharacter.IsDead && generalOptions.NameplateDeadPlayerHandling == DeadPlayerHandling.GrayOut)
            GrayOutNameplate(handler.PlayerCharacter, nameplateChanges);

        // Apply text color
        if (handler.PlayerCharacter != null && (!handler.PlayerCharacter.IsDead || generalOptions.NameplateDeadPlayerHandling == DeadPlayerHandling.Include))
        {
            Identity identity = pluginData.GetIdentity(handler.PlayerCharacter);
            foreach (var customTagId in identity.CustomTagIds)
            {
                var customTag = pluginData.CustomTags.FirstOrDefault(tag => tag.CustomId.Value == customTagId);
                if (customTag != null)
                    applyTextFormatting(customTag);
            }

            if (handler.PlayerCharacter.ClassJob.ValueNullable is ClassJob classJob && pluginData.JobTags.TryGetValue(classJob.Abbreviation.ParseString(), out var jobTag))
                applyTextFormatting(jobTag);

            void applyTextFormatting(Tag tag)
            {
                var dic = new Dictionary<NameplateElementChange, InheritableValue<bool>>
                {
                    { nameplateChanges.GetChange(NameplateElements.Name), tag.IsTextColorAppliedToNameplateName },
                    { nameplateChanges.GetChange(NameplateElements.Title), tag.IsTextColorAppliedToNameplateTitle },
                    { nameplateChanges.GetChange(NameplateElements.FreeCompany), tag.IsTextColorAppliedToNameplateFreeCompany },
                };
                ApplyTextFormatting(handler.PlayerCharacter, tag, dic, null);
            }
        }
    }

    private void GrayOutNameplate(IPlayerCharacter playerCharacter, NameplateChanges nameplateChanges)
    {
        foreach (var element in Enum.GetValues<NameplateElements>())
            nameplateChanges.GetChange(element).ApplyFormatting(new SeString().Append(new UIForegroundPayload(3)), new SeString().Append(new UIForegroundPayload(0)));
    }

    protected void ApplyTextFormatting(IPlayerCharacter? gameObject, Tag tag, IEnumerable<KeyValuePair<NameplateElementChange, InheritableValue<bool>>> changes, ushort? overwriteTextColor = null)
    {
        if (IsTagVisible(tag, gameObject))
        {
            foreach (var kvp in changes)
            {
                var change = kvp.Key;
                var enableFlag = kvp.Value;

                if (enableFlag.InheritedValue != null && enableFlag.InheritedValue.Value && (overwriteTextColor ?? tag.TextColor?.InheritedValue) is ushort colorToUse)
                    change.ApplyFormatting(new SeString().Append(new UIForegroundPayload(colorToUse)), new SeString().Append(new UIForegroundPayload(0)));
            }
        }
    }
}
