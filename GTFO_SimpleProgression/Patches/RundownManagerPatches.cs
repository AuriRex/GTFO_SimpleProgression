using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenCC.Utils;

namespace SimpleProgression.Patches
{


    /*[HarmonyWrapSafe]
    [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.NewGameSession))]
    public class RundownManager_NewGameSession_Patch
    {
        public static bool Prefix(RundownManager __instance)
        {
            if (__instance.m_activeDropServerGameSession != null)
            {
                Plugin.L.LogError("RundownManager.NewGameSession: Session already active! Ending current session before starting a new one.");
                __instance.EndGameSession();
            }
            if (!RundownManager.SessionGUIDSet)
            {
                return false;
            }

            Plugin.L.LogInfo("RundownManager: Starting new game session");

            List<uint> list = new List<uint>();
            for (int i = 0; i < 3; i++)
            {
                BoosterImplant activeBoosterImplant = PersistentInventoryManager.GetActiveBoosterImplant((global::BoosterImplantCategory)i);
                if (activeBoosterImplant != null)
                {
                    list.Add(activeBoosterImplant.InstanceId);
                }
            }
            __instance.m_activeDropServerGameSession = LocalGameSession.NewGameSession(RundownManager.SessionGUID, RundownManager.ActiveRundownKey, RundownManager.GetRundownProgressionExpeditionKey(RundownManager.GetActiveExpeditionData()), list.ToArray()).TryCast<IDropServerGameSession>();

            return false;
        }
    }

    [HarmonyWrapSafe]
    [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.EndGameSession))]
    public class RundownManager_EndGameSession_Patch
    {
        public static bool Prefix(RundownManager __instance)
        {
            Plugin.L.LogInfo("RundownManager.EndGameSession");
            if (__instance.m_activeDropServerGameSession == null)
            {
                Plugin.L.LogError("RundownManager.EndGameSession: No active game session");
                return false;
            }

            __instance.m_activeDropServerGameSession.EndSession();
            __instance.m_activeDropServerGameSession = null;

            *//*__instance.UpdateRundownProgression(RundownManager.ActiveRundownKey);
            PersistentInventoryManager.SetInventoryDirty(true);*//*
            return false;
        }
    }*/
}
