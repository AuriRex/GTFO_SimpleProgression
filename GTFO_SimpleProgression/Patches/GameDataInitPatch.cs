using GameData;
using HarmonyLib;
using SimpleProgression.Core;

namespace SimpleProgression.Patches;

[HarmonyPatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
public class GameDataInitPatch
{
    private static bool _first = true;
    public static void Postfix()
    {
        if (_first)
        {
            _first = false;
            return;
        }

        Plugin.L.Warning($"MTFO Hot-Relead triggered, reloading templates, groups and drop data ...");
        LocalVanityItemDropper.Instance.LoadTemplatesGroupsAndDropData();
        LocalBoosterDropper.Instance.Load();
    }
}