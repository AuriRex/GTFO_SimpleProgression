using AssetShards;
using HarmonyLib;

namespace SimpleProgression.Patches;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(AssetShardManager), nameof(AssetShardManager.Setup))]
internal class AssetShardManager__Setup__Patch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        Plugin.OnDataBlocksReady();
    }
}