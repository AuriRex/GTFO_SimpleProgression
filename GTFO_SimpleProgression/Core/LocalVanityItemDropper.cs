using GameData;
using SimpleProgression.Interfaces;
using SimpleProgression.Models.Vanity;
using System;
using System.Linq;

namespace SimpleProgression.Core;

public class LocalVanityItemDropper
{
    public static event Action<LocalVanityItemDropper> OnSetupDone;
    
    private static LocalVanityItemDropper _instance;
    public static LocalVanityItemDropper Instance => _instance ??= new LocalVanityItemDropper(Plugin.L);

    private readonly ILogger _logger;
    public bool IsSetup { get; private set; }

    private LocalVanityItemDropper(ILogger logger)
    {
        _logger = logger;
    }

    public VanityItemsGroupDataBlock[] ItemGroups { get; private set; }
    public VanityItemsTemplateDataBlock[] ItemTemplates { get; private set; }
    public VanityItemsLayerDropsDataBlock[] ItemDropData { get; private set; }


    private void InitCheck()
    {
        if (!IsSetup)
            throw new InvalidOperationException($"{nameof(LocalVanityItemDropper)} has not been initialized yet!");
    }

    internal void Init()
    {
        if (IsSetup)
        {
            _logger.Info($"{nameof(LocalVanityItemDropper)} already setup, skipping ...");
            return;
        }

        LoadTemplatesGroupsAndDropData();

        IsSetup = true;

        OnSetupDone?.Invoke(this);
    }

    internal void LoadTemplatesGroupsAndDropData()
    {
        ItemGroups = VanityItemsGroupDataBlock.GetAllBlocks();
        ItemTemplates = VanityItemsTemplateDataBlock.GetAllBlocks();
        ItemDropData = VanityItemsLayerDropsDataBlock.GetAllBlocks();

        if (ItemGroups.Length == 0 && ItemDropData.Length == 0)
        {
            _logger.Msg(ConsoleColor.Magenta, $"{nameof(LocalVanityItemDropper)}.{nameof(Init)}() complete, no groups or layer drops set! (Found {ItemTemplates.Length} Templates)");
            return;
        }

        _logger.Msg(ConsoleColor.Magenta, $"{nameof(LocalVanityItemDropper)}.{nameof(Init)}() complete, retrieved {ItemTemplates.Length} Templates, {ItemGroups.Length} Groups and {ItemDropData.Length} Layer Drops. Layer Drops:");
        foreach (var dd in ItemDropData)
        {
            _logger.Info($" > {dd.name}: #Drops: {dd.LayerDrops?.Count ?? 0}, Enabled: {dd.internalEnabled}");
        }
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

        if (!TryGetGroup(groupID, out var itemGroup) || itemGroup.HasAllOwned(playerData))
            return false;
        
        _logger.Msg(ConsoleColor.Magenta, $"Attempting drop of 1 Vanity Item from group \"{itemGroup.name}\" (ID:{groupID})");

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
            Flags = silentDrop ? LocalVanityItemStorage.VanityItemFlags.ALL : LocalVanityItemStorage.VanityItemFlags.None,
            // We use a custom key for any items in groups that aren't present in the vanilla game.
            CustomKey = IsCustomGroup(itemGroup) ? template.name : null,
        };

        playerData.Items.Add(item);
        _logger.Info($"Dropped Vanity Item \"{template?.publicName ?? $"ID:{itemId}"}\"!");
        return true;
    }

    public bool TryDropCustomItem(VanityItemsTemplateDataBlock template, bool silentDrop = false, bool doDropAlreadyOwnedItem = false)
    {
        InitCheck();

        var playerData = LocalVanityItemManager.Instance.LocalVanityItemPlayerData;
        
        if (doDropAlreadyOwnedItem || playerData.Items.FirstOrDefault(item => item.IsCustom && item.CustomKey == template.name) == null)
        {
            var item = new LocalVanityItemStorage.LocalVanityItem
            {
                CustomKey = template.name,
                ItemID = template.persistentID,
                Flags = silentDrop ? LocalVanityItemStorage.VanityItemFlags.ALL : LocalVanityItemStorage.VanityItemFlags.None
            };

            playerData.Items.Add(item);
            _logger.Info($"Dropped Custom Vanity Item \"{template?.publicName ?? $"ID:{template?.persistentID ?? 0}"}\"!");
            return true;
        }

        _logger.Info($"Aborted drop of Custom Vanity Item \"{template?.publicName ?? $"ID:{template?.persistentID ?? 0}"}\" because it's already owned!");
        return false;
    }

    public bool TryGetTemplate(uint persistentID, out VanityItemsTemplateDataBlock template)
    {
        InitCheck();

        template = ItemTemplates.FirstOrDefault(t => t.persistentID == persistentID);
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
        DropRandomFromGroup(3, playerData, silentDrop: true);
        DropRandomFromGroup(4, playerData, silentDrop: true);
        DropRandomFromGroup(5, playerData, silentDrop: true);
        DropRandomFromGroup(6, playerData, silentDrop: true);
        DropRandomFromGroup(7, playerData, silentDrop: true);
    }

    internal bool TryGetBlockFromCustomKey(string customKey, out VanityItemsTemplateDataBlock block)
    {
        block = ItemTemplates.FirstOrDefault(template => template.name == customKey);
        return block != null;
    }

    internal bool IsCustomGroup(uint groupId)
    {
        if (!TryGetGroup(groupId, out var group))
            return false;

        return IsCustomGroup(group);
    }
    
    internal bool IsCustomGroup(VanityItemsGroupDataBlock group)
    {
        return !AllVanityGroupsToUnlock.GAME_DEFAULT_GROUPS.Contains(group.name);
    }
}