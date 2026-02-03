using HarmonyLib;
using SimpleProgression.Core;

namespace SimpleProgression.Patches;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CheckpointManager), nameof(CheckpointManager.StoreCheckpoint))]
public class CheckpointManager__StoreCheckpoint__Patch
{
    public static void Prefix()
    {
        LocalProgressionManager.Instance.SaveAtCheckpoint();
    }
}

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CheckpointManager), nameof(CheckpointManager.ReloadCheckpoint))]
public class CheckpointManager__ReloadCheckpoint__Patch
{
    public static void Prefix()
    {
        LocalProgressionManager.Instance.ReloadFromCheckpoint();
    }
}