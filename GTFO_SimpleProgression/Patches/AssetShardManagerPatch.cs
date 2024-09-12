using AssetShards;
using HarmonyLib;

namespace SimpleProgression.Patches
{
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(AssetShardManager), nameof(AssetShardManager.Setup))]
    internal class LocalizationManager_Setup_Patch
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            Plugin.OnDataBlocksReady();
        }
    }
}
