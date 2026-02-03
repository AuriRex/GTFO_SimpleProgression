using HarmonyLib;
using SimpleProgression.Core;

namespace SimpleProgression.Patches;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(GS_InLevel), nameof(GS_InLevel.Enter))]
public class GS_InLevel__Enter__Patch
{
    public static void Postfix()
    {
        LocalProgressionManager.Instance.OnLevelEntered();
    }
}