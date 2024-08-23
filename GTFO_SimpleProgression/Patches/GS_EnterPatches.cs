using HarmonyLib;

namespace SimpleProgression.Patches
{
    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(GS_InLevel), nameof(GS_InLevel.Enter))]
    public class GS_InLevel_Enter_Patch
    {
        public static void Postfix()
        {
            LocalProgressionManager.Instance.OnLevelEntered();
        }
    }
}
