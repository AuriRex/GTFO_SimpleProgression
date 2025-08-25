using DropServer.VanityItems;
using Il2CppInterop.Runtime.Injection;
using SimpleProgression.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Clonesoft.Json;
using GameData;

namespace SimpleProgression.Models.Vanity;

public class LocalVanityItemStorage
{
    public List<LocalVanityItem> Items { get; set; } = new List<LocalVanityItem>();

    public void SetFlag(uint id, VanityItemFlags flag)
    {
        Items.Where(x => x.ItemID == id).ToList().ForEach(x => x.Flags |= flag);
    }

    public void UnsetFlag(uint id, VanityItemFlags flag)
    {
        Items.Where(x => x.ItemID == id).ToList().ForEach(x => x.Flags &= ~flag);
    }

    public IEnumerable<LocalVanityItem> GetValidItemsAndFixCustomIDs()
    {
        foreach (var item in Items)
        {
            if (item.IsCustom)
            {
                // IDs might be shifting on custom items, so we refresh the ID
                if (!LocalVanityItemDropper.Instance.TryGetBlockFromCustomKey(item.CustomKey, out var block))
                {
                    Plugin.L.Warning($"Could not find custom item \"{item.CustomKey}\"! It won't be in your inventory in game!");
                    continue;
                }
                item.ItemID = block.persistentID;
            }

            yield return item;
        }
    }


    public class LocalVanityItem
    {
        public uint ItemID { get; set; } = 0;
        public VanityItemFlags Flags { get; set; } = VanityItemFlags.None;
        
        [JsonIgnore]
        public bool IsCustom => !string.IsNullOrWhiteSpace(CustomKey);
        public string CustomKey { get; set; } = null;
    }

    [Flags]
    public enum VanityItemFlags
    {
        None = 0,
        Acknowledged = 1,
        Touched = 2,
        ALL = Acknowledged | Touched,
    }

    public VanityItemPlayerData ToBaseGame() => ToBaseGame(this);

    public static VanityItemPlayerData ToBaseGame(LocalVanityItemStorage customData)
    {
        var validItems = customData.GetValidItemsAndFixCustomIDs().ToList();

        if (Plugin.IsAllVanityLoaded)
        {
            AddNonOwnedBlocks(ref validItems);
        }
        
        var vipd = new VanityItemPlayerData(ClassInjector.DerivedConstructorPointer<VanityItemPlayerData>());

        vipd.Items = new(validItems.Count);

        for (int i = 0; i < validItems.Count; i++)
        {
            var current = validItems[i];

            var item = new DropServer.VanityItems.VanityItem()
            {
                ItemId = current.ItemID,
                Flags = (InventoryItemFlags)current.Flags
            };

            vipd.Items[i] = item;
        }

        return vipd;
    }

    private static void AddNonOwnedBlocks(ref List<LocalVanityItem> validItems)
    {
        var groupDBNames = LocalVanityItemManager.Instance.AllVanityGroupsToUnlock.GroupsToUnlock;

        HashSet<uint> allTemplateIds = new();
        
        foreach (var block in VanityItemsGroupDataBlock.GetAllBlocks())
        {
            if (!groupDBNames.Contains(block.name))
                continue;

            foreach (var item in block.Items)
            {
                allTemplateIds.Add(item);
            }
        }
        
        var list = validItems;
        var otherBlocks = VanityItemsTemplateDataBlock.GetAllBlocks().Where(block => allTemplateIds.Contains(block.persistentID) && list.All(vi => vi.ItemID != block.persistentID));
        foreach (var block in otherBlocks)
        {
            validItems.Add(new LocalVanityItem()
            {
                ItemID = block.persistentID,
                Flags = VanityItemFlags.ALL
            });
        }
    }

    public static LocalVanityItemStorage FromBaseGame(VanityItemPlayerData vanityPlayerData)
    {
        var items = new List<LocalVanityItem>();

        foreach (var item in vanityPlayerData.Items)
        {
            items.Add(new LocalVanityItem()
            {
                ItemID = item.ItemId,
                Flags = (VanityItemFlags)item.Flags
            });
        }

        return new LocalVanityItemStorage()
        {
            Items = items
        };
    }
}