using HarmonyLib;

namespace SimpleProgression.Patches
{
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.Setup))]
    internal class LocalizationManager_Setup_Patch
    {
        public static void Postfix()
        {
            Plugin.OnDataBlocksReady();
        }
    }
}
