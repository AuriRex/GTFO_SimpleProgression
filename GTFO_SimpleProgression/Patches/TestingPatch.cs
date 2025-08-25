#if DEBUG
using HarmonyLib;

namespace SimpleProgression.Patches;

[HarmonyPatch(typeof(RundownManager), nameof(RundownManager.OnExpeditionEnded))]
public class TestingPatch
{
    public static void Prefix()
    {
        // f*ck you (respectfully) <3
        // Essentially, the game only sends the expedition result request if you've been in the level
        // for at least 30 seconds, and that's kinda annoying when you're in the process of
        // testing local vanity drop behaviour etc.
        Clock.ExpeditionProgressionTime = 31f; 
    }
}
#endif