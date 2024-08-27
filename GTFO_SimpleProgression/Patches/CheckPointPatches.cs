using HarmonyLib;
using SimpleProgression.Core;

namespace SimpleProgression.Patches
{
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(CheckpointManager), nameof(CheckpointManager.StoreCheckpoint))]
    public class CheckpointManager_StoreCheckpoint_Patch
    {
        public static void Prefix()
        {
            LocalProgressionManager.Instance.SaveAtCheckpoint();
        }
    }

    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(CheckpointManager), nameof(CheckpointManager.ReloadCheckpoint))]
    public class CheckpointManager_ReloadCheckpoint_Patch
    {
        public static void Prefix()
        {
            LocalProgressionManager.Instance.ReloadFromCheckpoint();
        }
    }
}
