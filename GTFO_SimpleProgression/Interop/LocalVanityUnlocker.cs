using GameData;
using SimpleProgression.Models.Progression;
using System;
using System.Collections.Generic;

namespace SimpleProgression.Interop
{
    public static class LocalVanityUnlocker
    {
        internal static List<Func<ExpeditionCompletionData?, IEnumerable<VanityItemsTemplateDataBlock>>> unlockMethods = new();

        public static void RegisterUnlockMethod(Func<ExpeditionCompletionData?, IEnumerable<VanityItemsTemplateDataBlock>> unlockMethod)
        {
            unlockMethods.Add(unlockMethod);
        }
    }
}
