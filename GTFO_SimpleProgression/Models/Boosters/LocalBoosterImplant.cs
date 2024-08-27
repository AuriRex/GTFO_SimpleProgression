using Clonesoft.Json;
using DropServer.BoosterImplants;
using GameData;
using Il2CppInterop.Runtime.Injection;
using System;

namespace SimpleProgression.Models.Boosters
{
    public class LocalBoosterImplant
    {
        public LocalBoosterImplant()
        {

        }

        // DropServer variant constructor
        public LocalBoosterImplant(uint templateId, uint instanceId, int uses, Effect[] effects, uint[] conditions)
        {
            TemplateId = templateId;
            InstanceId = instanceId;
            Uses = uses;

            Effects = effects ?? Array.Empty<Effect>();
            Conditions = conditions ?? Array.Empty<uint>();
        }

        public static void CopyTraitColorsFromBasegame()
        {
            TraitConditionColor = BoosterImplant.TraitConditionColor;
            TraitNegativeColor = BoosterImplant.TraitNegativeColor;
            TraitPositiveColor = BoosterImplant.TraitPositiveColor;
        }

        protected static DropServer.BoosterImplants.BoosterImplantInventoryItem ToBaseGame(LocalDropServerBoosterImplantInventoryItem custom, DropServer.BoosterImplants.BoosterImplantInventoryItem baseGame)
        {
            var implant = new DropServer.BoosterImplants.BoosterImplantInventoryItem(ClassInjector.DerivedConstructorPointer<DropServer.BoosterImplants.BoosterImplantInventoryItem>());

            implant.Effects = new BoosterImplantEffect[custom.Effects.Length];
            for (int i = 0; i < custom.Effects.Length; i++)
            {
                var fx = custom.Effects[i];
                implant.Effects[i] = new BoosterImplantEffect
                {
                    Id = fx.Id,
                    Param = fx.Value
                };
            }
            implant.TemplateId = custom.TemplateId;
            implant.Id = custom.InstanceId;
            implant.Conditions = new uint[custom.Conditions.Length];
            for (int i = 0; i < custom.Conditions.Length; i++)
            {
                implant.Conditions[i] = custom.Conditions[i];
            }
            implant.UsesRemaining = custom.Uses;

            return implant;
        }

        public static string TraitConditionColor { get; private set; }
        public static string TraitNegativeColor { get; private set; }
        public static string TraitPositiveColor { get; private set; }

        public uint TemplateId { get; set; } // Readonly in basegame
        public SaveBoosterImplantCategory Category { get; set; } // Readonly in basegame
        public int Uses { get; set; }
        public Effect[] Effects { get; set; }
        [JsonIgnore]
        public BoosterImplantTemplateDataBlock Template { get; set; }
        public uint InstanceId { get; set; }
        public uint[] Conditions { get; set; }

        public struct Effect
        {
            public uint Id;
            public float Value;
        }

        public enum SaveBoosterImplantCategory
        {
            Muted,
            Bold,
            Aggressive
        }
    }
}
