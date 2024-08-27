using Clonesoft.Json;
using DropServer.BoosterImplants;
using Il2CppInterop.Runtime.Injection;

namespace SimpleProgression.Models.Boosters
{
    public class LocalDropServerBoosterImplantInventoryItem : LocalBoosterImplant
    {
        [JsonConstructor]
        public LocalDropServerBoosterImplantInventoryItem() : base()
        {

        }

        public LocalDropServerBoosterImplantInventoryItem(uint templateId, uint instanceId, int uses, Effect[] effects, uint[] conditions) : base(templateId, instanceId, uses, effects, conditions)
        {

        }

        public DropServer.BoosterImplants.BoosterImplantInventoryItem ToBaseGame() => ToBaseGame(this);

        public static DropServer.BoosterImplants.BoosterImplantInventoryItem ToBaseGame(LocalDropServerBoosterImplantInventoryItem custom)
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

            implant.Flags = custom.Flags;

            return implant;
        }

        public static LocalDropServerBoosterImplantInventoryItem FromBaseGame(DropServer.BoosterImplants.BoosterImplantInventoryItem implant)
        {
            var customInventoryItem = new LocalDropServerBoosterImplantInventoryItem();


            LocalBoosterImplant.Effect[] effects = new LocalBoosterImplant.Effect[implant.Effects.Length];

            var customImplant = new LocalBoosterImplant();

            for (int i = 0; i < implant.Effects.Length; i++)
            {
                var efx = implant.Effects[i];
                effects[i] = new LocalBoosterImplant.Effect
                {
                    Value = efx.Param,
                    Id = efx.Id,
                };
            }

            customImplant.TemplateId = implant.TemplateId;
            customImplant.InstanceId = implant.Id;
            customImplant.Uses = implant.UsesRemaining;
            customImplant.Effects = effects;
            customImplant.Conditions = implant.Conditions;


            customInventoryItem.Flags = implant.Flags;

            return customInventoryItem;
        }

        public uint Flags { get; set; } = 0;
        
        [JsonIgnore]
        public bool IsTouched
        {
            get
            {
                return (Flags & 1) != 0;
            }
            set
            {
                if(value)
                {
                    Flags |= 1;
                }
                else
                {
                    uint tmp = ~(uint)1;
                    Flags &= tmp;
                }
            }
        }
    }
}
