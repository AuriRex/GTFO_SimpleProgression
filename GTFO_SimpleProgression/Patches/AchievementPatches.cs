using HarmonyLib;

namespace SimpleProgression.Patches;

[HarmonyPatch(typeof(Achievement_ReadAllLogs), nameof(Achievement_ReadAllLogs.CheckCompletion))]
public static class Achievement_ReadAllLogs__CheckCompletion__Patch
{
    public static bool Prefix(Achievement_ReadAllLogs __instance)
    {
        if (__instance.m_allLogs.Count != Achievement_ReadAllLogs.TOTAL_LOGS_FOR_ACHIEVEMENT)
        {
            // We skip in case we aren't running vanilla log counts ig
            return false;
        }
        
        if (SteamManager.Current.GetAchievement(__instance.AchievementKey))
        {
            // ReadAllLogs is unlocked, so we don't need to do anything and cancel OG
            return false;
        }
        return true;
    }
}