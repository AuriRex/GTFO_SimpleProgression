using Clonesoft.Json;
using DropServer;
using DropServer.BoosterImplants;
using SimpleProgression.Interfaces;
using SimpleProgression.Models.Boosters;
using System;
using System.IO;
using System.Linq;

namespace SimpleProgression.Core
{
    public class LocalBoosterManager
    {
        private static LocalBoosterManager _instance;
        public static LocalBoosterManager Instance => _instance ??= new LocalBoosterManager(Plugin.L);

        private readonly ILogger _logger;

        public bool Disabled { get; internal set; } = false;

        private LocalBoosterManager(ILogger logger)
        {
            _logger = logger;
        }

        public static bool DoConsumeBoosters { get; set; } = true;

        private static LocalBoosterImplantPlayerData _localBoosterImplantPlayerData = null;
        public static LocalBoosterImplantPlayerData LocalBoosterImplantPlayerData => _localBoosterImplantPlayerData ??= LoadFromBoosterFile();

        public static uint[] BoostersToBeConsumed { get; private set; } = Array.Empty<uint>();


        public static LocalBoosterDropper BoosterDropper => LocalBoosterDropper.Instance;

        internal static void SaveBoostersToDisk()
        {
            SaveToBoosterFile(LocalBoosterImplantPlayerData);
        }

        // called everytime a new booster is selected for the first time to update the value / missed boosters are aknowledged / a booster has been dropped
        public BoosterImplantPlayerData UpdateBoosterImplantPlayerData(BoosterImplantTransaction transaction) // returns basegame BoosterImplantPlayerData
        {
            if (Instance.Disabled)
            {
                return LocalBoosterImplantPlayerData.ToBaseGame();
            }

            if (transaction.DropIds != null)
                LocalBoosterImplantPlayerData.DropBoostersWithIds(transaction.DropIds.ToArray());

            if (transaction.TouchIds != null)
                LocalBoosterImplantPlayerData.SetBoostersTouchedWithIds(transaction.TouchIds.ToArray());

            if (transaction.AcknowledgeIds != null)
                LocalBoosterImplantPlayerData.AcknowledgeBoostersWithIds(transaction.AcknowledgeIds.ToArray());

            if (transaction.AcknowledgeMissed != null)
                LocalBoosterImplantPlayerData.AcknowledgeMissedBoostersWithIds(transaction.AcknowledgeMissed);

            SaveBoostersToDisk();
            return LocalBoosterImplantPlayerData.ToBaseGame();
        }

        public BoosterImplantPlayerData GetBoosterImplantPlayerData(uint maxBackendTemplateId)
        {
            SaveBoostersToDisk();
            return LocalBoosterImplantPlayerData.ToBaseGame();
        }

        public void ConsumeBoosters(string sessionBlob)
        {
            if (Instance.Disabled)
            {
                return;
            }

            if (DoConsumeBoosters)
            {
                // remove boosters from the file & save

                LocalBoosterImplantPlayerData.ConsumeBoostersWithIds(BoostersToBeConsumed);

                SaveBoostersToDisk();
            }

            // clear used boosters list or something
            BoostersToBeConsumed = Array.Empty<uint>();
        }

        public void EndSession(EndSessionRequest.PerBoosterCategoryInt boosterCurrency/*, bool success, string sessionBlob, uint maxBackendBoosterTemplateId, int buildRev*/)
        {
            if (Instance.Disabled)
            {
                return;
            }

            LocalBoosterImplantPlayerData.AddCurrency(boosterCurrency);

            // Test if currency exceeds 1000, remove that amount and generate & add a new randomly created booster
            // or add 1 to the Missed Counter if inventory is full (10 items max)
            var cats = LocalBoosterImplantPlayerData.GetCategoriesWhereCurrencyCostHasBeenReached();

            foreach(var cat in cats)
            {
                while(cat.HasEnoughCurrencyForDrop)
                {
                    cat.Currency -= LocalBoosterImplantPlayerData.CurrencyNewBoosterCost;

                    if (cat.InventoryIsFull)
                    {
                        _logger.Warning($"Inventory full, missed 1 {cat.CategoryType} booster.");
                        cat.Missed++;
                        continue;
                    }

                    _logger.Notice($"Generating 1 {cat.CategoryType} booster ... [CurrencyRemaining:{cat.Currency}]");
                    BoosterDropper.GenerateAndAddBooster(ref _localBoosterImplantPlayerData, cat.CategoryType);
                }
                
            }


            SaveBoostersToDisk();
        }

        public void StartSession(uint[] boosterIds, string sessionId)
        {
            BoostersToBeConsumed = boosterIds;
        }

        public static void SaveToBoosterFile(LocalBoosterImplantPlayerData data)
        {
            if (Instance.Disabled)
            {
                return;
            }

            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Instance._logger.Msg(ConsoleColor.DarkRed, $"Saving boosters to disk at: {Paths.BoostersFilePath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Paths.BoostersFilePath, json);
        }

        public static LocalBoosterImplantPlayerData LoadFromBoosterFile()
        {
            if (Instance.Disabled)
            {
                return new LocalBoosterImplantPlayerData();
            }

            Instance._logger.Msg(ConsoleColor.Green, $"Loading boosters from disk at: {Paths.BoostersFilePath}");
            if (!File.Exists(Paths.BoostersFilePath))
                return new LocalBoosterImplantPlayerData();
            var json = File.ReadAllText(Paths.BoostersFilePath);

            return JsonConvert.DeserializeObject<LocalBoosterImplantPlayerData>(json);
        }
    }
}
