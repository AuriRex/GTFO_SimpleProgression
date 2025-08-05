using DropServer;
using HarmonyLib;
using SimpleProgression.Impl;

namespace SimpleProgression.Patches;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.OnTitleDataUpdated))]
internal class DropServerManager__OnTitleDataUpdated__Patch
{
    public static bool Prefix(DropServerManager __instance)
    {
        __instance.ClientApi = new LocalDropServerAPI().TryCast<IDropServerClientAPI>();

        return false;
    }
}

[HarmonyWrapSafe]
[HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.GetStatusText))]
internal class DropServerManager__GetStatusText__Patch
{
    public static bool Prefix(DropServerManager __instance, ref string __result)
    {
        if (!__instance.IsBusy)
        {
            __result = null;
            return false;
        }
        __result = "STORAGE SYNC";
        return false;
    }
}