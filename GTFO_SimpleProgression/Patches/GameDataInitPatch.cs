using System;
using GameData;
using HarmonyLib;
using SimpleProgression.Core;

namespace SimpleProgression.Patches;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
public class GameDataInit__Initialize__Patch
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
        try
        {
            LocalVanityItemDropper.Instance.LoadTemplatesGroupsAndDropData();
            LocalBoosterDropper.Instance.Load();
        }
        catch(Exception ex)
        {
            Plugin.L.Exception(ex);
        }
    }
}