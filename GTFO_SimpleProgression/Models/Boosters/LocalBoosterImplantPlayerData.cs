using Clonesoft.Json;
using DropServer;
using DropServer.BoosterImplants;
using Il2CppInterop.Runtime.Injection;
using System.Collections.Generic;
using System.Linq;
using static SimpleProgression.Models.Boosters.LocalBoosterImplant;

namespace SimpleProgression.Models.Boosters
{
    public class LocalBoosterImplantPlayerData
    {
        public static int CurrencyNewBoosterCost { get; set; } = 1000;
        public static float CurrencyGainMultiplier { get; set; } = 1f;

        /// <summary>
        /// Muted Boosters
        /// </summary>
        public CustomCategory Basic { get; set; } = new CustomCategory(SaveBoosterImplantCategory.Muted);
        /// <summary>
        /// Bold Boosters
        /// </summary>
        public CustomCategory Advanced { get; set; } = new CustomCategory(SaveBoosterImplantCategory.Bold);
        /// <summary>
        /// Agressive Boosters
        /// </summary>
        public CustomCategory Specialized { get; set; } = new CustomCategory(SaveBoosterImplantCategory.Aggressive);
        /// <summary>
        /// Array of Boosters InstanceIds that are new (in game popup in lobby screen)
        /// </summary>
        public uint[] New { get; set; } = new uint[0];

        public LocalBoosterImplantPlayerData() { }

        /// <summary>
        /// Acknowledge the amount of boosters missed
        /// </summary>
        /// <param name="acknowledgeMissed"></param>
        public void AcknowledgeMissedBoostersWithIds(BoosterImplantTransaction.Missed acknowledgeMissed)
        {
            Basic.MissedAck = acknowledgeMissed.Basic;
            Advanced.MissedAck = acknowledgeMissed.Advanced;
            Specialized.MissedAck = acknowledgeMissed.Specialized;
        }

        /// <summary>
        /// Get all categories where a new booster should be generated for.
        /// </summary>
        /// <returns></returns>
        public CustomCategory[] GetCategoriesWhereCurrencyCostHasBeenReached()
        {
            var cats = new List<CustomCategory>();

            if (Basic.Currency >= CurrencyNewBoosterCost) cats.Add(Basic);
            if (Advanced.Currency >= CurrencyNewBoosterCost) cats.Add(Advanced);
            if (Specialized.Currency >= CurrencyNewBoosterCost) cats.Add(Specialized);

            return cats.ToArray();
        }

        /// <summary>
        /// Acknowledge newly aquired boosters. (Done by closing the popup in game)
        /// </summary>
        /// <param name="boostersToAcknowledge"></param>
        public void AcknowledgeBoostersWithIds(uint[] boostersToAcknowledge)
        {
            var newNew = new List<uint>();
            foreach(var id in New)
            {
                if(boostersToAcknowledge.Any(idToAcknowledge => id == idToAcknowledge))
                {
                    continue;
                }
                newNew.Add(id);
            }
            New = newNew.ToArray();
        }

        /// <summary>
        /// Remove the (!) and "New" indicators in game by "touching" or interacting with the booster.
        /// </summary>
        /// <param name="boostersThatWereTouched"></param>
        public void SetBoostersTouchedWithIds(uint[] boostersThatWereTouched)
        {
            Basic.SetBoostersTouchedWithIds(boostersThatWereTouched);
            Advanced.SetBoostersTouchedWithIds(boostersThatWereTouched);
            Specialized.SetBoostersTouchedWithIds(boostersThatWereTouched);
        }

        /// <summary>
        /// Use up 1 charge nad remove if they're used up
        /// </summary>
        /// <param name="boostersToBeConsumed"></param>
        public void ConsumeBoostersWithIds(uint[] boostersToBeConsumed)
        {
            Basic.ConsumeOrDropBoostersWithIds(boostersToBeConsumed);
            Advanced.ConsumeOrDropBoostersWithIds(boostersToBeConsumed);
            Specialized.ConsumeOrDropBoostersWithIds(boostersToBeConsumed);
        }

        /// <summary>
        /// Drop as in remove those boosters from the inventory
        /// </summary>
        /// <param name="boostersToBeDropped"></param>
        public void DropBoostersWithIds(uint[] boostersToBeDropped)
        {
            Basic.DropBoostersWithIds(boostersToBeDropped);
            Advanced.DropBoostersWithIds(boostersToBeDropped);
            Specialized.DropBoostersWithIds(boostersToBeDropped);
        }

        public void AddCurrency(EndSessionRequest.PerBoosterCategoryInt boosterCurrency)
        {
            Basic.Currency += (int) (boosterCurrency.Basic * CurrencyGainMultiplier);
            Advanced.Currency += (int) (boosterCurrency.Advanced * CurrencyGainMultiplier);
            Specialized.Currency += (int) (boosterCurrency.Specialized * CurrencyGainMultiplier);
        }

        /// <summary>
        /// Add Booster into the Category and set it as new.
        /// </summary>
        /// <param name="newBooster"></param>
        /// <returns>true if the Booster has been added</returns>
        public bool TryAddBooster(LocalDropServerBoosterImplantInventoryItem newBooster)
        {
            if(!GetCategory(newBooster.Category).TryAddBooster(newBooster))
                return false;

            var newNew = new uint[New.Length + 1];

            for(int i = 0; i < New.Length; i++)
            {
                newNew[i] = New[i];
            }

            newNew[New.Length] = newBooster.InstanceId;

            New = newNew;

            return true;
        }

        public uint[] GetUsedIds()
        {
            var cats = GetAllCategories();

            var usedIds = new List<uint>();
            foreach (var cat in cats)
            {
                foreach(var id in cat.GetUsedIds())
                {
                    usedIds.Add(id);
                }
            }

            return usedIds.ToArray();
        }

        public CustomCategory[] GetAllCategories()
        {
            return new CustomCategory[]
            {
                Basic,
                Advanced,
                Specialized
            };
        }

        public CustomCategory GetCategory(SaveBoosterImplantCategory category)
        {
            switch(category)
            {
                case SaveBoosterImplantCategory.Muted:
                    return Basic;
                case SaveBoosterImplantCategory.Bold:
                    return Advanced;
                case SaveBoosterImplantCategory.Aggressive:
                    return Specialized;
            }
            return null;
        }

        public BoosterImplantPlayerData ToBaseGame() => ToBaseGame(this);

        public static BoosterImplantPlayerData ToBaseGame(LocalBoosterImplantPlayerData customData)
        {
            var playerData = new BoosterImplantPlayerData(ClassInjector.DerivedConstructorPointer<BoosterImplantPlayerData>());

            playerData.New = customData.New;

            playerData.Basic = customData.Basic.ToBaseGame();
            playerData.Advanced = customData.Advanced.ToBaseGame();
            playerData.Specialized = customData.Specialized.ToBaseGame();

            return playerData;
        }

        public static LocalBoosterImplantPlayerData FromBaseGame(BoosterImplantPlayerData data)
        {
            var customData = new LocalBoosterImplantPlayerData();

            customData.Basic = CustomCategory.FromBaseGame(data.Basic);
            customData.Basic.CategoryType = SaveBoosterImplantCategory.Muted;

            customData.Advanced = CustomCategory.FromBaseGame(data.Advanced);
            customData.Advanced.CategoryType = SaveBoosterImplantCategory.Bold;

            customData.Specialized = CustomCategory.FromBaseGame(data.Specialized);
            customData.Specialized.CategoryType = SaveBoosterImplantCategory.Aggressive;

            customData.New = new uint[data.New.Count];
            for (int i = 0; i < data.New.Count; i++)
            {
                customData.New[i] = data.New[i];
            }

            return customData;
        }

        public class CustomCategory
        {
            [JsonIgnore]
            public const int MAX_BOOSTERS_R5 = 10;
            [JsonIgnore]
            public const int MAX_BOOSTERS_R6 = 20;

            // Helper
            public SaveBoosterImplantCategory CategoryType { get; set; } = SaveBoosterImplantCategory.Muted;

            private int _currency = 0;
            /// <summary> 1000 -> 100% -> new booster </summary>
            public int Currency
            {
                get => _currency;
                set => _currency = value > 0 ? value : 0;
            }
            /// <summary> Number of missed boosters </summary>
            public int Missed { get; set; } = 0;
            /// <summary> Number of missed boosters that have been acknowledged by the player (displays missed boosters popup ingame if unequal with <see cref="Missed"/>) </summary>
            public int MissedAck { get; set; } = 0;

            public LocalDropServerBoosterImplantInventoryItem[] Inventory { get; set; } = new LocalDropServerBoosterImplantInventoryItem[0];

            public CustomCategory() { }

            public CustomCategory(SaveBoosterImplantCategory cat)
            {
                CategoryType = cat;
            }

            [JsonIgnore]
            public static int MaxBoostersInCategoryInventory => GetMaxBoostersInCategory();

            [JsonIgnore]
            public bool InventoryIsFull => Inventory.Length >= MaxBoostersInCategoryInventory;

            [JsonIgnore]
            public bool HasEnoughCurrencyForDrop => Currency >= LocalBoosterImplantPlayerData.CurrencyNewBoosterCost;


            public uint[] GetUsedIds()
            {
                uint[] usedIds = new uint[Inventory.Length];
                for (int i = 0; i < Inventory.Length; i++)
                {
                    usedIds[i] = Inventory[i].InstanceId;
                }
                return usedIds;
            }

            /// <summary>
            /// Add a Booster into this categories inventory.
            /// </summary>
            /// <param name="newBooster"></param>
            /// <returns>true if the booster has been added</returns>
            public bool TryAddBooster(LocalDropServerBoosterImplantInventoryItem newBooster)
            {
                if (InventoryIsFull) return false;

                var newInventory = new LocalDropServerBoosterImplantInventoryItem[Inventory.Length + 1];

                for (int i = 0; i < Inventory.Length; i++)
                {
                    newInventory[i] = Inventory[i];
                }

                newInventory[newInventory.Length - 1] = newBooster;

                Inventory = newInventory;

                return true;
            }

            internal void ConsumeOrDropBoostersWithIds(uint[] boostersToBeConsumed)
            {
                var newInventory = new List<LocalDropServerBoosterImplantInventoryItem>();

                foreach(var item in Inventory)
                {
                    if (boostersToBeConsumed.Any(toConsumeId => item.InstanceId == toConsumeId))
                    {
                        if (item.Uses > 1)
                            item.Uses -= 1; // consumes one charge
                        else
                            continue; // removes the Booster on last charge
                    }

                    newInventory.Add(item);
                }

                Inventory = newInventory.ToArray();
            }

            internal void DropBoostersWithIds(uint[] boostersToBeDropped)
            {
                var newInventory = new List<LocalDropServerBoosterImplantInventoryItem>();

                foreach (var item in Inventory)
                {
                    if (boostersToBeDropped.Any(toDropId => item.InstanceId == toDropId))
                    {
                        continue;
                    }

                    newInventory.Add(item);
                }

                Inventory = newInventory.ToArray();
            }

            internal void SetBoostersTouchedWithIds(uint[] boostersThatWereTouched)
            {
                foreach (var item in Inventory)
                {
                    if (boostersThatWereTouched.Any(toTouchId => item.InstanceId == toTouchId))
                    {
                        item.IsTouched = true;
                    }
                }
            }

            public BoosterImplantPlayerData.Category ToBaseGame() => ToBaseGame(this);

            public static BoosterImplantPlayerData.Category ToBaseGame(CustomCategory customCat)
            {
                var cat = new BoosterImplantPlayerData.Category(ClassInjector.DerivedConstructorPointer<BoosterImplantPlayerData.Category>());

                cat.Currency = customCat.Currency;
                cat.Missed = customCat.Missed;

                cat.Inventory = new(customCat.Inventory.Length);
                for (int i = 0; i < customCat.Inventory.Length; i++)
                {
                    cat.Inventory[i] = customCat.Inventory[i].ToBaseGame();
                }

                return cat;
            }

            public static CustomCategory FromBaseGame(BoosterImplantPlayerData.Category cat)
            {
                var customCat = new CustomCategory();

                customCat.Currency = cat.Currency;
                customCat.Missed = cat.Missed;
                customCat.MissedAck = cat.MissedAck;

                var customItems = new List<LocalDropServerBoosterImplantInventoryItem>();
                foreach (var item in cat.Inventory)
                {
                    customItems.Add(LocalDropServerBoosterImplantInventoryItem.FromBaseGame(item));
                }
                customCat.Inventory = customItems.ToArray();

                return customCat;
            }

            public static int GetMaxBoostersInCategory()
            {
                return MAX_BOOSTERS_R6;
            }
        }
    }
}
