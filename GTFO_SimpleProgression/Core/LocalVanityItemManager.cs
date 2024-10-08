﻿using Clonesoft.Json;
using DropServer.VanityItems;
using GameData;
using SimpleProgression.Interfaces;
using SimpleProgression.Interop;
using SimpleProgression.Models.Progression;
using SimpleProgression.Models.Vanity;
using System;
using System.IO;

namespace SimpleProgression.Core
{
    public class LocalVanityItemManager
    {
        private static LocalVanityItemManager _instance;
        public static LocalVanityItemManager Instance => _instance ??= new LocalVanityItemManager(Plugin.L);

        private readonly ILogger _logger;

        private LocalVanityItemManager(ILogger logger)
        {
            _logger = logger;

            LocalProgressionManager.OnExpeditionCompleted += OnExpeditionCompleted;
        }

        private LocalVanityItemStorage _localVanityItemStorage;
        public LocalVanityItemStorage LocalVanityItemPlayerData
        {
            get
            {
                if (_localVanityItemStorage == null)
                {
                    // Load from disk
                    _localVanityItemStorage = LoadFromLocalFile();

                    if (_localVanityItemStorage.Items.Count == 0)
                    {
                        Dropper.DropFirstTimePlayingItems(_localVanityItemStorage);
                    }

                    CheckCustomVanityUnlockConditionsMet(null);
                }

                return _localVanityItemStorage;
            }
        }

        private LocalVanityAcquiredLayerDrops _acquiredLayerDrops;
        public LocalVanityAcquiredLayerDrops AlreadyAcquiredLayerDrops => _acquiredLayerDrops ??= LoadAcquiredLayerDrops();

        private LocalVanityItemDropper Dropper => LocalVanityItemDropper.Instance;

        public void OnExpeditionCompleted(ExpeditionCompletionData data)
        {
            CheckCustomVanityUnlockConditionsMet(data);

            if (!data.Success)
                return;

            CheckFirstTimeExpeditionCompletion(data);
            CheckTotalUniqueCompletionsRequirementMet(data);
        }

        internal void CheckCustomVanityUnlockConditionsMet(ExpeditionCompletionData? data)
        {
            foreach(var method in LocalVanityUnlocker.unlockMethods)
            {
                try
                {
                    var toDrop = method?.Invoke(data);

                    foreach(var template in toDrop)
                    {
                        Dropper.TryDropCustomItem(LocalVanityItemPlayerData, template);
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error($"Custom Vanity Unlock Condition threw an exception (continuing ...):");
                    _logger.Exception(ex);
                }
            }
        }

        public void CheckTotalUniqueCompletionsRequirementMet(ExpeditionCompletionData data)
        {
            try
            {
                if (data.RundownId == 0)
                {
                    _logger.Error($"[{nameof(CheckTotalUniqueCompletionsRequirementMet)}] {nameof(ExpeditionCompletionData)}.{nameof(ExpeditionCompletionData.RundownId)} returned 0!");
                    return;
                }

                var vanityItemLayerDropDataBlockPersistentID = RundownDataBlock.GetBlock(data.RundownId)?.VanityItemLayerDropDataBlock;

                if (!vanityItemLayerDropDataBlockPersistentID.HasValue)
                {
                    _logger.Warning($"[{nameof(CheckTotalUniqueCompletionsRequirementMet)}] {nameof(vanityItemLayerDropDataBlockPersistentID)} has no value!");
                    return;
                }

                VanityItemsLayerDropsDataBlock vilddb = VanityItemsLayerDropsDataBlock.GetBlock(vanityItemLayerDropDataBlockPersistentID.Value);

                if (vilddb == null)
                {
                    _logger.Warning($"[{nameof(CheckTotalUniqueCompletionsRequirementMet)}] {nameof(VanityItemsLayerDropsDataBlock)} with persistent ID {vanityItemLayerDropDataBlockPersistentID} could not be found!");
                    return;
                }

                bool anyDropped = false;

                foreach (var layerDropData in vilddb.LayerDrops)
                {
                    var layer = layerDropData.Layer.ToCustom();
                    var count = layerDropData.Count;
                    var isAll = layerDropData.IsAll;

                    string key = $"{vilddb.name}:{layer}_{count}_{isAll}";

                    if (LocalProgressionManager.Instance.CurrentLoadedLocalProgressionData.GetUniqueExpeditionLayersStateCount(layer) >= count)
                    {
                        if (!AlreadyAcquiredLayerDrops.HasBeenClaimed(key))
                        {
                            _logger.Notice($"Dropping layer milestone reached rewards for \"{key}\" ...");
                            foreach (var group in layerDropData.Groups)
                            {
                                anyDropped |= Dropper.DropRandomFromGroup(group, LocalVanityItemPlayerData);
                            }

                            AlreadyAcquiredLayerDrops.Claim(key);
                        }
                    }
                }

                if (anyDropped)
                {
                    SaveAcquiredLayerDrops(AlreadyAcquiredLayerDrops);
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Checking for unique layer drops completion rewards failed!");
                _logger.Exception(ex);
            }
        }

        public void CheckFirstTimeExpeditionCompletion(ExpeditionCompletionData data)
        {
            if (!data.WasFirstTimeCompletion)
                return;

            try
            {
                if (data.RundownId == 0)
                {
                    _logger.Error($"[{nameof(CheckFirstTimeExpeditionCompletion)}] {nameof(ExpeditionCompletionData)}.{nameof(ExpeditionCompletionData.RundownId)} returned 0!");
                    return;
                }

                RundownDataBlock rddb = RundownDataBlock.GetBlock(data.RundownId);

                DropFirstTimeCompletionRewards(rddb.GetExpeditionData(data.ExpeditionTier, data.ExpeditionIndex));
            }
            catch(Exception ex)
            {
                _logger.Error("Something went wrong:");
                _logger.Exception(ex);
            }
        }

        public void DropFirstTimeCompletionRewards(GameData.ExpeditionInTierData expeditionData)
        {
            if (expeditionData.VanityItemsDropData.Groups.Count > 0)
            {
                _logger.Notice("Dropping first time completion rewards ...");
                foreach (var group in expeditionData.VanityItemsDropData.Groups)
                {
                    Dropper.DropRandomFromGroup(group, LocalVanityItemPlayerData);
                }
            }
        }

        public bool HasUnlocked(VanityItemsTemplateDataBlock block)
        {
            if (block == null)
                return false;

            foreach (var item in LocalVanityItemPlayerData.Items)
            {
                if (item.IsCustom)
                {
                    if (item.CustomKey == block.name)
                        return true;
                    continue;
                }

                if (item.ItemID == block.persistentID)
                    return true;
            }

            return false;
        }

        public VanityItemPlayerData ProcessTransaction(VanityItemServiceTransaction trans)
        {
            if(trans != null)
            {
                if(trans.AcknowledgeIds != null) AcknowledgeIds(trans.AcknowledgeIds);
                if(trans.TouchIds != null) TouchIds(trans.TouchIds);
            }

            return GetVanityItemPlayerData();
        }

        public VanityItemPlayerData GetVanityItemPlayerData()
        {
            SaveToLocalFile(LocalVanityItemPlayerData);
            return LocalVanityItemPlayerData.ToBaseGame();
        }

        public void AcknowledgeIds(uint[] ids)
        {
            foreach(var id in ids)
            {
                LocalVanityItemPlayerData.SetFlag(id, LocalVanityItemStorage.VanityItemFlags.Acknowledged);
            }
        }

        public void TouchIds(uint[] ids)
        {
            foreach (var id in ids)
            {
                LocalVanityItemPlayerData.SetFlag(id, LocalVanityItemStorage.VanityItemFlags.Touched);
            }
        }

        public static void SaveToLocalFile(LocalVanityItemStorage data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Instance._logger.Msg(ConsoleColor.DarkRed, $"Saving VanityItems to disk at: {Paths.VanityItemsFilePath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Paths.VanityItemsFilePath, json);
        }

        public static LocalVanityItemStorage LoadFromLocalFile()
        {
            Instance._logger.Msg(ConsoleColor.Green, $"Loading VanityItems from disk at: {Paths.VanityItemsFilePath}");
            if (!File.Exists(Paths.VanityItemsFilePath))
                return new LocalVanityItemStorage();
            var json = File.ReadAllText(Paths.VanityItemsFilePath);

            return JsonConvert.DeserializeObject<LocalVanityItemStorage>(json);
        }

        public static void SaveAcquiredLayerDrops(LocalVanityAcquiredLayerDrops data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Instance._logger.Fail($"Saving LocalVanityAcquiredLayerDrops to disk at: {Paths.VanityItemsLayerDropsPath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Paths.VanityItemsLayerDropsPath, json);
        }

        public static LocalVanityAcquiredLayerDrops LoadAcquiredLayerDrops()
        {
            Instance._logger.Success($"Loading LocalVanityAcquiredLayerDrops from disk at: {Paths.VanityItemsLayerDropsPath}");
            if (!File.Exists(Paths.VanityItemsLayerDropsPath))
                return new LocalVanityAcquiredLayerDrops();
            var json = File.ReadAllText(Paths.VanityItemsLayerDropsPath);

            return JsonConvert.DeserializeObject<LocalVanityAcquiredLayerDrops>(json);
        }
    }
}
