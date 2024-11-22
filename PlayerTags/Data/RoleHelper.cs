using Lumina.Excel.Sheets;
using Pilz.Dalamud;
using System.Collections.Generic;
using System.Linq;

namespace PlayerTags.Data;

public static class RoleHelper
{
    public static Dictionary<byte, Role> RolesByRoleId { get; } = new Dictionary<byte, Role>()
    {
        { 0, Role.LandHand },
        { 1, Role.Tank },
        { 2, Role.Dps },
        { 3, Role.Dps },
        { 4, Role.Healer },
    };

    public static Dictionary<byte, DpsRole> DpsRolesByRoleId { get; } = new Dictionary<byte, DpsRole>()
    {
        { 2, DpsRole.Melee },
        { 3, DpsRole.Ranged },
    };

    public static Dictionary<byte, RangedDpsRole> RangedDpsRolesByPrimaryStat { get; } = new Dictionary<byte, RangedDpsRole>()
    {
        { 4, RangedDpsRole.Magical },
        { 2, RangedDpsRole.Physical },
    };

    private static Dictionary<string, Role>? s_RolesByJobAbbreviation = null;
    public static Dictionary<string, Role> RolesByJobAbbreviation
    {
        get
        {
            if (s_RolesByJobAbbreviation == null)
            {
                s_RolesByJobAbbreviation = [];

                var classJobs = PluginServices.DataManager.GetExcelSheet<ClassJob>();
                if (classJobs != null)
                {
                    foreach (var classJob in classJobs.Where(classJob => !string.IsNullOrEmpty(classJob.Abbreviation.ParseString())))
                    {
                        if (RolesByRoleId.TryGetValue(classJob.Role, out var role))
                        {
                            s_RolesByJobAbbreviation[classJob.Abbreviation.ParseString()] = role;
                        }
                    }
                }
            }

            return s_RolesByJobAbbreviation;
        }
    }

    private static Dictionary<string, DpsRole>? s_DpsRolesByJobAbbreviation = null;
    public static Dictionary<string, DpsRole> DpsRolesByJobAbbreviation
    {
        get
        {
            if (s_DpsRolesByJobAbbreviation == null)
            {
                s_DpsRolesByJobAbbreviation = [];

                var classJobs = PluginServices.DataManager.GetExcelSheet<ClassJob>();
                if (classJobs != null)
                {
                    foreach (var classJob in classJobs.Where(classJob => !string.IsNullOrEmpty(classJob.Abbreviation.ParseString())))
                    {
                        if (DpsRolesByRoleId.TryGetValue(classJob.Role, out var dpsRole))
                        {
                            s_DpsRolesByJobAbbreviation[classJob.Abbreviation.ParseString()] = dpsRole;
                        }
                    }
                }
            }

            return s_DpsRolesByJobAbbreviation;
        }
    }

    private static Dictionary<string, RangedDpsRole>? s_RangedDpsRolesByJobAbbreviation = null;
    public static Dictionary<string, RangedDpsRole> RangedDpsRolesByJobAbbreviation
    {
        get
        {
            if (s_RangedDpsRolesByJobAbbreviation == null)
            {
                s_RangedDpsRolesByJobAbbreviation = [];

                var classJobs = PluginServices.DataManager.GetExcelSheet<ClassJob>();
                if (classJobs != null)
                {
                    foreach (var classJob in classJobs.Where(classJob => !string.IsNullOrEmpty(classJob.Abbreviation.ParseString())))
                    {
                        if (DpsRolesByJobAbbreviation.TryGetValue(classJob.Abbreviation.ParseString(), out var dpsRole) && dpsRole == DpsRole.Ranged)
                        {
                            if (RangedDpsRolesByPrimaryStat.TryGetValue(classJob.PrimaryStat, out var rangedDPSRole))
                            {
                                s_RangedDpsRolesByJobAbbreviation[classJob.Abbreviation.ParseString()] = rangedDPSRole;
                            }
                        }
                    }
                }
            }

            return s_RangedDpsRolesByJobAbbreviation;
        }
    }

    private static Dictionary<string, LandHandRole>? s_LandHandRolesByJobAbbreviation = null;
    public static Dictionary<string, LandHandRole> LandHandRolesByJobAbbreviation
    {
        get
        {
            if (s_LandHandRolesByJobAbbreviation == null)
            {
                s_LandHandRolesByJobAbbreviation = [];

                var classJobs = PluginServices.DataManager.GetExcelSheet<ClassJob>();
                var gatheringSubCategories = PluginServices.DataManager.GetExcelSheet<GatheringSubCategory>();
                if (classJobs != null && gatheringSubCategories != null)
                {
                    var gatheringJobAbbreviations = gatheringSubCategories
                        .Select(gatheringSubCategory => gatheringSubCategory.ClassJob.Value)
                        .Select(classJob => classJob!.Abbreviation).Distinct();

                    foreach (var classJob in classJobs.Where(classJob => !string.IsNullOrEmpty(classJob.Abbreviation.ParseString())))
                    {
                        if (RolesByRoleId.TryGetValue(classJob.Role, out var role))
                        {
                            if (role == Role.LandHand)
                            {
                                if (gatheringJobAbbreviations.Contains(classJob.Abbreviation))
                                {
                                    s_LandHandRolesByJobAbbreviation[classJob.Abbreviation.ParseString()] = LandHandRole.Land;
                                }
                                else
                                {
                                    s_LandHandRolesByJobAbbreviation[classJob.Abbreviation.ParseString()] = LandHandRole.Hand;
                                }
                            }
                        }
                    }
                }
            }

            return s_LandHandRolesByJobAbbreviation;
        }
    }
}
