using System;
using HarmonyLib;
using IL2Tasks = Il2CppSystem.Threading.Tasks;
using IL2System = Il2CppSystem;
using Il2ColGen = Il2CppSystem.Collections.Generic;
using static SimpleProgression.Core.PlayFabFilesManager;

namespace SimpleProgression.Patches;

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.TryGetRundownTimerData))]
internal static class PlayFabManager__TryGetRundownTimerData__Patch
{
    public static bool Prefix(ref bool __result, out RundownTimerData data)
    {
        data = new RundownTimerData();
        data.ShowScrambledTimer = true;
        data.ShowCountdownTimer = true;
        var theDate = DateTime.Today.AddDays(20);
        data.UTC_Target_Day = theDate.Day;
        data.UTC_Target_Hour = theDate.Hour;
        data.UTC_Target_Minute = theDate.Minute;
        data.UTC_Target_Month = theDate.Month;
        data.UTC_Target_Year = theDate.Year;

        __result = true;
        return SKIP_OG;
    }
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.TryGetStartupScreenData))]
internal static class PlayFabManager__TryGetStartupScreenData__Patch
{
    public static bool Prefix(ref bool __result, out StartupScreenData data)
    {
        data = new StartupScreenData();
        data.AllowedToStartGame = true;
        data.ShowOvertoneButton = false;
        data.IntroText = "Startup Override :)";
        data.ShowBugReportButton = false;
        data.ShowIntroText = false;
        data.ShowRoadmapButton = false;
        data.ShowDiscordButton = false;

        __result = true;
        
        return SKIP_OG;
    }
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.OnGetAuthSessionTicketResponse))]
internal static class PlayFabManager__OnGetAuthSessionTicketResponse__Patch
{
    public static string PLAYFAB_ID = "idk_lol";

    private static string _entityId;
    public static string EntityID => _entityId ??= "Player_" + new System.Random().Next(int.MinValue, int.MaxValue);

    private static string _entityToken;
    public static string EntityToken => _entityToken ??= "EntityToken_" + new System.Random().Next(int.MinValue, int.MaxValue);

    public static bool Prefix()
    {
        Plugin.L.Notice("Tricking the game into thinking we're logged in ...");

        var playFabManager = PlayFabManager.Current;
        
        playFabManager.m_globalTitleDataLoaded = true;
        playFabManager.m_playerDataLoaded = true;
        playFabManager.m_entityId = EntityID;
        playFabManager.m_entityType = "Player";
        playFabManager.m_entityToken = EntityToken;
        playFabManager.m_entityLoggedIn = true;

        playFabManager.m_globalTitleData = new();

        PlayFabManager.PlayFabId = PLAYFAB_ID;

        PlayFabManager.LoggedInDateTime = new IL2System.DateTime();
        PlayFabManager.LoggedInSeconds = Clock.Time;

        OnLoggedIn();

        PlayFabManager.GlobalTitleData["DropServer"] = "https://localhost:12345";

        PlayFabManager.OnLoginSuccess?.Invoke();
        PlayFabManager.OnTitleDataUpdated?.Invoke();

        return SKIP_OG;
    }
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.GetEntityTokenAsync))]
internal static class PlayFabManager__GetEntityTokenAsync__Patch
{
    public static bool Prefix(ref IL2Tasks.Task<string> __result)
    {
        __result = IL2Tasks.Task.FromResult<string>(PlayFabManager__OnGetAuthSessionTicketResponse__Patch.EntityToken);

        return SKIP_OG;
    }
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshGlobalTitleDataForKeys))]
internal static class PlayFabManager__RefreshGlobalTitleDataForKeys_Patch
{
    public static bool Prefix(Il2ColGen.List<string> keys, IL2System.Action OnSuccess)
    {
        if (keys != null)
        {
            foreach (var key in keys)
            {
                Plugin.L.Msg(ConsoleColor.DarkYellow, $"RefreshGlobalTitleDataForKeys -> Key:{key}");
            }
        }

        OnSuccess?.Invoke();

        return SKIP_OG;
    }
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.AddToOrUpdateLocalPlayerTitleData), new Type[] { typeof(string), typeof(string), typeof(IL2System.Action) })]
internal static class PlayFabManager__AddToOrUpdateLocalPlayerTitleData__Patch
{
    public static bool Prefix(string key, string value, IL2System.Action OnSuccess)
    {
        Plugin.L.Debug($"Canceled AddToOrUpdateLocalPlayerTitleData: Key:{key} - Value:{value}");

        UpdateLocalPlayerTitleData(key, value);
        
        OnSuccess?.Invoke();

        return SKIP_OG;
    }
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.AddToOrUpdateLocalPlayerTitleData), new Type[] { typeof(Il2ColGen.Dictionary<string, string>), typeof(IL2System.Action) })]
internal static class PlayFabManager__AddToOrUpdateLocalPlayerTitleDataOverload__Patch
{
    public static bool Prefix(Il2ColGen.Dictionary<string, string> keys, IL2System.Action OnSuccess)
    {
        Plugin.L.Debug($"Canceled AddToOrUpdateLocalPlayerTitleData(OverloadMethod): Count:{keys?.Count}");

        if (keys != null)
        {
            foreach (var kvp in keys)
            {
                UpdateLocalPlayerTitleData(kvp.Key, kvp.Value);
            }
        }

        OnSuccess?.Invoke();

        return SKIP_OG;
    }
}

#region NotAsImportantPatches
[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.CloudGiveAlwaysInInventory))]
internal static class PlayFabManager__CloudGiveAlwaysInInventory__Patch
{
    public static bool Prefix(IL2System.Action onSucess) => SkipOriginalAndInvoke(onSucess);
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.CloudGiveItemToLocalPlayer))]
internal static class PlayFabManager__CloudGiveItemToLocalPlayer__Patch
{
    public static bool Prefix(string ItemId, IL2System.Action onSucess) => SkipOriginalAndInvoke(onSucess);
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.JSONTest))]
internal static class PlayFabManager__JSONTest__Patch
{
    public static bool Prefix() => SKIP_OG;
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshItemCatalog))]
internal static class PlayFabManager__RefreshItemCatalog__Patch
{
    public static bool Prefix(PlayFabManager.delUpdateItemCatalogDone OnSuccess, string catalogVersion) => SKIP_OG;
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshLocalPlayerInventory))]
internal static class PlayFabManager__RefreshLocalPlayerInventory__Patch
{
    public static bool Prefix(PlayFabManager.delUpdatePlayerInventoryDone OnSuccess) => SKIP_OG;
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshLocalPlayerTitleData))]
internal static class PlayFabManager__RefreshLocalPlayerTitleData__Patch
{
    public static bool Prefix(IL2System.Action OnSuccess) => SkipOriginalAndInvoke(OnSuccess);
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshGlobalTitleData))]
internal static class PlayFabManager__RefreshGlobalTitleData__Patch
{
    public static bool Prefix(IL2System.Action OnSuccess) => SkipOriginalAndInvoke(OnSuccess);
}

[HarmonyPatch(typeof(PlayFabManager), nameof(PlayFabManager.RefreshStoreItems))]
internal static class PlayFabManager__RefreshStoreItems__Patch
{
    public static bool Prefix(string storeID, PlayFabManager.delUpdateStoreItemsDone OnSuccess)
    {
        OnSuccess?.Invoke(null);
        return SKIP_OG;
    }
}
#endregion NotAsImportantPatches
