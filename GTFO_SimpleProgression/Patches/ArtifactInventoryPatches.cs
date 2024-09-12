using BoosterImplants;
using HarmonyLib;
using SimpleProgression.Core;

namespace SimpleProgression.Patches
{
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(ArtifactInventory), nameof(ArtifactInventory.OnStateChange))]
    public static class ArtifactInventory_OnStateChange_Patch
    {
        public static void Postfix(ArtifactInventory __instance)
        {
            LocalProgressionManager.Instance.ArtifactCountUpdated(__instance.CommonCount, __instance.UncommonCount, __instance.RareCount);
        }
    }
}
