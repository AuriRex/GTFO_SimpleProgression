using GameData;
using SimpleProgression.Interfaces;
using SimpleProgression.Models.Boosters;
using System;
using System.Collections.Generic;
using System.Linq;
using static SimpleProgression.Models.Boosters.LocalBoosterImplant;

namespace SimpleProgression.Core
{
    public class LocalBoosterDropper
    {
        private static LocalBoosterDropper _instance;
        public static LocalBoosterDropper Instance => _instance ??= new LocalBoosterDropper(Plugin.L);

        public bool Inited { get; private set; } = false;

        private readonly ILogger _logger;

        private LocalBoosterDropper(ILogger logger)
        {
            _logger = logger;
        }

        public BoosterImplantTemplateDataBlock[] MutedTemplates { get; private set; }
        public BoosterImplantTemplateDataBlock[] BoldTemplates { get; private set; }
        public BoosterImplantTemplateDataBlock[] AgrressiveTemplates { get; private set; }

        public BoosterImplantEffectDataBlock[] Effects { get; private set; }
        public BoosterImplantConditionDataBlock[] Conditions { get; private set; }

        public void Init()
        {
            if (Inited)
            {
                _logger.Info($"{nameof(LocalBoosterDropper)} already setup, skipping ...");
                return;
            }

            _logger.Info($"Setting up {nameof(LocalBoosterDropper)} ...");

            BoosterImplantTemplateDataBlock[] templates = BoosterImplantTemplateDataBlock.GetAllBlocks();

            MutedTemplates = templates.Where(t => t.ImplantCategory == BoosterImplantCategory.Muted).ToArray();
            BoldTemplates = templates.Where(t => t.ImplantCategory == BoosterImplantCategory.Bold).ToArray();
            AgrressiveTemplates = templates.Where(t => t.ImplantCategory == BoosterImplantCategory.Aggressive).ToArray();

            Effects = BoosterImplantEffectDataBlock.GetAllBlocks();
            Conditions = BoosterImplantConditionDataBlock.GetAllBlocks();

            if(MutedTemplates.Length == 0 && BoldTemplates.Length == 0 && AgrressiveTemplates.Length == 0)
            {
                LocalBoosterManager.Instance.Disabled = true;
                _logger.Msg(ConsoleColor.Magenta, $"{nameof(LocalBoosterDropper)}.{nameof(Init)}() complete, no templates set -> boosters disabled!");
            }
            else
            {
                _logger.Msg(ConsoleColor.Magenta, $"{nameof(LocalBoosterDropper)}.{nameof(Init)}() complete, retrieved {MutedTemplates.Length} Muted, {BoldTemplates.Length} Bold and {AgrressiveTemplates.Length} Agrressive Templates as well as {Effects?.Length} Effects and {Conditions?.Length} Conditions.");
            }

            Inited = true;
        }

        public static int BOOSTER_DROP_MAX_REROLL_COUNT { get; internal set; } = 25;

        private void InitCheck()
        {
            if (!Inited)
                throw new InvalidOperationException($"{nameof(LocalVanityItemDropper)} has not been initialized yet!");
        }

        public LocalDropServerBoosterImplantInventoryItem GenerateBooster(SaveBoosterImplantCategory category, uint[] usedIds)
        {
            InitCheck();

            BoosterImplantTemplateDataBlock template;
            float weight = 1f;

            int maxUses;

            int count = 0;
            do
            {
                switch (category)
                {
                    default:
                    case SaveBoosterImplantCategory.Muted:
                        template = MutedTemplates[UnityEngine.Random.Range(0, MutedTemplates.Length)];
                        break;
                    case SaveBoosterImplantCategory.Bold:
                        template = BoldTemplates[UnityEngine.Random.Range(0, BoldTemplates.Length)];
                        break;
                    case SaveBoosterImplantCategory.Aggressive:
                        template = AgrressiveTemplates[UnityEngine.Random.Range(0, AgrressiveTemplates.Length)];
                        break;
                }
                if (template == null) continue;
                if (count > BOOSTER_DROP_MAX_REROLL_COUNT) break;
                weight = 1f;
                if (template.DropWeight != 0) weight = 1 / template.DropWeight;
                count++;
            } while (template == null || UnityEngine.Random.Range(0f, 1f) > weight);

            switch (category)
            {
                default:
                case SaveBoosterImplantCategory.Muted:
                    maxUses = 1; // 1
                    break;
                case SaveBoosterImplantCategory.Bold:
                    maxUses = UnityEngine.Random.Range(1, 3); // 1-2
                    break;
                case SaveBoosterImplantCategory.Aggressive:
                    maxUses = UnityEngine.Random.Range(2, 4); // 2-3
                    break;
            }

            var conditionIds = new List<uint>();
            var effects = new List<LocalBoosterImplant.Effect>();

            // Add set effects
            foreach (var fx in template.Effects)
            {
                effects.Add(new LocalBoosterImplant.Effect
                {
                    Id = fx.BoosterImplantEffect,
                    Value = UnityEngine.Random.Range(fx.MinValue, fx.MaxValue)
                });
            }

            // Choose from random effects
            foreach (var randomEffectList in template.RandomEffects)
            {
                if (randomEffectList == null || randomEffectList.Count < 1) continue;
                var fx = randomEffectList[UnityEngine.Random.Range(0, randomEffectList.Count)];

                effects.Add(new LocalBoosterImplant.Effect
                {
                    Id = fx.BoosterImplantEffect,
                    Value = UnityEngine.Random.Range(fx.MinValue, fx.MaxValue)
                });
            }

            // Add set condition
            foreach (var cond in template.Conditions)
            {
                conditionIds.Add(cond);
            }

            // Pick one of the random conditions
            if (template.RandomConditions != null && template.RandomConditions.Count > 0)
            {
                conditionIds.Add(template.RandomConditions[UnityEngine.Random.Range(0, template.RandomConditions.Count)]);
            }

            var instanceId = GenerateInstanceId(usedIds);

            var value = new LocalDropServerBoosterImplantInventoryItem(template.persistentID, instanceId, maxUses, effects.ToArray(), conditionIds.ToArray());

            value.Category = category;

            value.Template = template;

            BoosterImplantEffectDataBlock effectDB = null;
            BoosterImplantConditionDataBlock conditionDB = null;
            try
            {
                if(effects.Count > 0)
                    effectDB = Effects.First(ef => ef.persistentID == effects[0].Id);
                if(conditionIds.Count > 0)
                    conditionDB = Conditions.First(cd => cd.persistentID == conditionIds[0]);
            }
            catch(Exception)
            {

            }

            _logger.Msg(ConsoleColor.Magenta, $"Generated booster: {template.PublicName} - {effectDB?.PublicShortName} {conditionDB?.PublicShortName} ({effectDB?.PublicName} when {conditionDB?.PublicName})");

            return value;
        }

        private static uint GenerateInstanceId(uint[] usedIds)
        {
            if (usedIds == null || usedIds.Length == 0) return 1;
            uint count = 1;
            while(true)
            {
                if(usedIds.Any(i => i == count)) {
                    count++;
                }
                else
                {
                    return count;
                }
            }
        }

        public void GenerateAndAddBooster(ref LocalBoosterImplantPlayerData data, SaveBoosterImplantCategory category)
        {
            InitCheck();

            var newBooster = GenerateBooster(category, data.GetUsedIds());

            if(!data.TryAddBooster(newBooster))
            {
                _logger.Info($"Did not add Booster as the inventory for category {category} is full! (This message should not appear!)");
            }
        }
    }
}
