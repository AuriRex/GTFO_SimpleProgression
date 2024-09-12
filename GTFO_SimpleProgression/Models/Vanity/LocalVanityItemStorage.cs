using DropServer.VanityItems;
using Il2CppInterop.Runtime.Injection;
using SimpleProgression.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleProgression.Models.Vanity
{
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
            public bool IsCustom { get; set; } = false;
            public string CustomKey { get; set; } = string.Empty;
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
}
