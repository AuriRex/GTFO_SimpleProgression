using GameData;
using SimpleProgression.Interfaces;
using SimpleProgression.Models.Vanity;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SimpleProgression.Core
{
    public class LocalVanityItemDropper
    {
        private static LocalVanityItemDropper _instance;
        public static LocalVanityItemDropper Instance => _instance ??= new LocalVanityItemDropper(Plugin.L);

        private readonly ILogger _logger;
        public bool Inited { get; private set; } = false;

        private LocalVanityItemDropper(ILogger logger)
        {
            _logger = logger;
        }

        public VanityItemsGroupDataBlock[] ItemGroups { get; private set; }
        public VanityItemsTemplateDataBlock[] ItemTemplates { get; private set; }
        public VanityItemsLayerDropsDataBlock[] ItemDropData { get; private set; }


        private void InitCheck()
        {
            if (!Inited)
                throw new InvalidOperationException($"{nameof(LocalVanityItemDropper)} has not been initialized yet!");
        }

        internal void Init()
        {
            if (Inited)
            {
                _logger.Info($"{nameof(LocalVanityItemDropper)} already setup, skipping ...");
                return;
            }

            ItemGroups = VanityItemsGroupDataBlock.GetAllBlocks();
            ItemTemplates = VanityItemsTemplateDataBlock.GetAllBlocks();
            ItemDropData = VanityItemsLayerDropsDataBlock.GetAllBlocks();

            if (ItemGroups.Length == 0 && ItemDropData.Length == 0)
            {
                LocalVanityItemManager.Instance.Disabled = true;
                _logger.Msg(ConsoleColor.Magenta, $"{nameof(LocalVanityItemDropper)}.{nameof(Init)}() complete, no groups or layer drops set -> disabling vanity");
            }
            else
            {
                _logger.Msg(ConsoleColor.Magenta, $"{nameof(LocalVanityItemDropper)}.{nameof(Init)}() complete, retrieved {ItemTemplates.Length} Templates, {ItemGroups.Length} Groups and {ItemDropData.Length} Layer Drops. Layer Drops:");
                foreach (var dd in ItemDropData)
                {
                    _logger.Info($" > {dd.name}: #Drops: {dd.LayerDrops?.Count ?? 0}, Enabled: {dd.internalEnabled}");
                }
            }
            

            Inited = true;
        }

        public bool TryGetGroup(uint persistentID, out VanityItemsGroupDataBlock itemGroup)
        {
            InitCheck();

            foreach (var group in ItemGroups)
            {
                if(group.persistentID == persistentID)
                {
                    itemGroup = group;
                    return true;
                }
            }

            itemGroup = null;
            return false;
        }

        /// <summary>
        /// Drop (Add) an item from group with id <paramref name="groupID"/> into the players inventory <paramref name="playerData"/>
        /// </summary>
        /// <param name="groupID">The group to pick from</param>
        /// <param name="playerData">The players inventory data</param>
        /// <param name="silentDrop">If the game should announce that something new dropped</param>
        /// <returns>True if anything dropped</returns>
        public bool DropRandomFromGroup(uint groupID, LocalVanityItemStorage playerData, bool silentDrop = false)
        {
            InitCheck();

            if (TryGetGroup(groupID, out var itemGroup) && !itemGroup.HasAllOwned(playerData))
            {
                _logger.Msg(ConsoleColor.Magenta, $"Attempting drop of 1 Vanity Item from group \"{itemGroup.name}\"");

                if(!itemGroup.GetNonOwned(playerData).TryPickRandom(out var itemId))
                {
                    _logger.Info($"All items in group already in local player inventory, not dropping!");
                    return false;
                }

                if(!TryGetTemplate(itemId, out var template))
                {
                    _logger.Warning($"Template with ID {itemId} wasn't found!");
                }

                var item = new LocalVanityItemStorage.LocalVanityItem
                {
                    ItemID = itemId,
                    Flags = silentDrop ? LocalVanityItemStorage.VanityItemFlags.ALL : LocalVanityItemStorage.VanityItemFlags.None
                };

                playerData.Items.Add(item);
                _logger.Info($"Dropped Vanity Item \"{template?.publicName ?? $"ID:{itemId}"}\"!");
                return true;
            }
            return false;
        }

        public bool TryGetTemplate(uint persistentID, out VanityItemsTemplateDataBlock template)
        {
            InitCheck();

            template = ItemTemplates.Where(t => t.persistentID == persistentID).FirstOrDefault();
            return template != null;
        }

        public bool HasAllItemsInGroup(uint groupID, LocalVanityItemStorage playerData)
        {
            InitCheck();

            if (TryGetGroup(groupID, out var itemGroup))
            {
                return itemGroup.HasAllOwned(playerData);
            }
            return false;
        }

        internal void DropFirstTimePlayingItems(LocalVanityItemStorage playerData)
        {
            InitCheck();

            _logger.Warning("Dropping initial Vanity Items ...");
            DropRandomFromGroup(3, playerData, true);
            DropRandomFromGroup(4, playerData, true);
            DropRandomFromGroup(5, playerData, true);
            DropRandomFromGroup(6, playerData, true);
            DropRandomFromGroup(7, playerData, true);
        }
    }
}
