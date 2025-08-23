using GameData;
using SimpleProgression.Models.Vanity;
using System.Collections.Generic;
using System.Linq;

namespace SimpleProgression;

public static class Extensions
{
    public static bool TryPickRandom<T>(this IEnumerable<T> enumerable, out T value)
    {
        var array = enumerable.ToArray();
        
        if (array.Length == 0)
        {
            value = default;
            return false;
        }

        value = array[UnityEngine.Random.Range(0, array.Length)];
        return true;
    }

    #region VanityItemsGroupDataBlock
    public static IEnumerable<uint> GetNonOwned(this VanityItemsGroupDataBlock self, LocalVanityItemStorage playerData)
    {
        foreach (var item in self.Items)
        {
            if (playerData.Items.All(i => i.ItemID != item))
            {
                yield return item;
            }
        }
    }

    public static bool HasAllOwned(this VanityItemsGroupDataBlock self, LocalVanityItemStorage playerData)
    {
        foreach (var itemId in self.Items)
        {
            if (playerData.Items.All(i => i.ItemID != itemId))
                return false;
        }
        return true;
    }
    #endregion

}