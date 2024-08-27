using DropServer.VanityItems;
using Il2CppInterop.Runtime.Injection;
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



        public class LocalVanityItem
        {
            public uint ItemID { get; set; } = 0;
            public VanityItemFlags Flags { get; set; } = VanityItemFlags.None;
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
            var vipd = new VanityItemPlayerData(ClassInjector.DerivedConstructorPointer<VanityItemPlayerData>());

            vipd.Items = new (customData.Items.Count);

            for (int i = 0; i < customData.Items.Count; i++)
            {
                var current = customData.Items[i];

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
