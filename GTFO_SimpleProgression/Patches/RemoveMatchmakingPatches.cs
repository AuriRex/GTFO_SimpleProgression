using CellMenu;
using HarmonyLib;
using UnityEngine;

namespace SimpleProgression.Patches;

[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.Setup))]
public static class CM_PageRundown_New__Setup__Patch
{
    public static readonly Vector3 YEET_VECTOR = new Vector3(-5000, -20000, 0);
    
    public static void Postfix(CM_PageRundown_New __instance)
    {
        __instance.m_matchmakeAllButton.gameObject.transform.position = YEET_VECTOR;
    }
}

[HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.SetupFromPage))]
public static class CM_PlayerLobbyBar__SetupFromPage__Patch
{
    public static void Postfix(CM_PlayerLobbyBar __instance)
    {
        __instance.m_matchmakeButton.gameObject.transform.position = CM_PageRundown_New__Setup__Patch.YEET_VECTOR;
        __instance.m_matchmakeIcon.gameObject.transform.position = CM_PageRundown_New__Setup__Patch.YEET_VECTOR;
    }
}