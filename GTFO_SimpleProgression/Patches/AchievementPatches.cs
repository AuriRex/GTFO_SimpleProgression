using HarmonyLib;

namespace SimpleProgression.Patches;

[HarmonyPatch(typeof(Achievement_ReadAllLogs), nameof(Achievement_ReadAllLogs.CheckCompletion))]
public static class Achievement_ReadAllLogs__CheckCompletion__Patch
{
    public static bool Prefix(Achievement_ReadAllLogs __instance)
    {
        if (SteamManager.Current.GetAchievement(__instance.AchievementKey))
        {
            // ReadAllLogs is unlocked, so we don't need to do anything and cancel OG
            return false;
        }
        return true;
    }
}