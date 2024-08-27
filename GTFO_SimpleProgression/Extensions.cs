using GameData;
using SimpleProgression.Models.Vanity;
using System.Collections.Generic;
using System.Linq;

namespace SimpleProgression
{
    public static class Extensions
    {
        public static bool TryPickRandom<T>(this List<T> list, out T value)
        {
            if (list.Count == 0)
            {
                value = default;
                return false;
            }

            value = list[UnityEngine.Random.RandomRangeInt(0, list.Count - 1)];
            return true;
        }

        #region VanityItemsGroupDataBlock
        public static List<uint> GetNonOwned(this VanityItemsGroupDataBlock self, LocalVanityItemStorage playerData)
        {
            var value = new List<uint>();
            foreach (var item in self.Items)
            {
                if (!playerData.Items.Any(i => i.ItemID == item))
                {
                    value.Add(item);
                }
            }
            return value;
        }

        public static bool HasAllOwned(this VanityItemsGroupDataBlock self, LocalVanityItemStorage playerData)
        {
            foreach (var itemId in self.Items)
            {
                if (!playerData.Items.Any(i => i.ItemID == itemId))
                    return false;
            }
            return true;
        }
        #endregion

    }
}
