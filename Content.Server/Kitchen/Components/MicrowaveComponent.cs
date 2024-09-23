using Content.Shared.Construction.Prototypes;
using Content.Shared.DeviceLinking;
using Content.Shared.Item;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Kitchen.Components
{
    [RegisterComponent]
    public sealed partial class MicrowaveComponent : Component
    {
        [DataField]
        public float CookTimeMultiplier = 1;
        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartCookTimeMultiplier = "Capacitor";
        [DataField]
        public float CookTimeScalingConstant = 0.5f;
        [DataField]
        public float BaseHeatMultiplier = 100;

        [DataField]
        public float ObjectHeatMultiplier = 100;

        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string BadRecipeEntityId = "FoodBadRecipe";

        #region  audio
        [DataField]
        public SoundSpecifier StartCookingSound = new SoundPathSpecifier("/Audio/Machines/microwave_start_beep.ogg");

        [DataField]
        public SoundSpecifier FoodDoneSound = new SoundPathSpecifier("/Audio/Machines/microwave_done_beep.ogg");

        [DataField]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

        [DataField]
        public SoundSpecifier ItemBreakSound = new SoundPathSpecifier("/Audio/Effects/clang.ogg");

        public EntityUid? PlayingStream;

        [DataField]
        public SoundSpecifier LoopingSound = new SoundPathSpecifier("/Audio/Machines/microwave_loop.ogg");
        #endregion

        [ViewVariables]
        public bool Broken;

        [DataField]
        public ProtoId<SinkPortPrototype> OnPort = "On";

        /// <summary>
        ///     This is a fixed offset of 5.
        ///     The cook times for all recipes should be divisible by 5,with a minimum of 1 second.
        ///     For right now, I don't think any recipe cook time should be greater than 60 seconds.
        /// </summary>
        [DataField]
        public uint CurrentCookTimerTime = 0;

        /// <summary>
        ///     Tracks the elapsed time of the current cook timer.
        /// </summary>
        [DataField]
        public TimeSpan CurrentCookTimeEnd = TimeSpan.Zero;

        /// <summary>
        ///     The maximum number of seconds a microwave can be set to.
        ///     This is currently only used for validation and the client does not check this.
        /// </summary>
        [DataField]
        public uint MaxCookTime = 30;

        /// <summary>
        ///     The max temperature that this microwave can heat objects to.
        /// </summary>
        [DataField]
        public float TemperatureUpperThreshold = 373.15f;

        public int CurrentCookTimeButtonIndex;

        public Container Storage = default!;

        [DataField]
        public int Capacity = 10;

        [DataField]
        public ProtoId<ItemSizePrototype> MaxItemSize = "Normal";

        /// <summary>
        ///     How frequently the microwave can malfunction.
        /// </summary>
        [DataField]
        public float MalfunctionInterval = 1.0f;

        /// <summary>
        ///     Chance of an explosion occurring when we microwave a metallic object
        /// </summary>
        [DataField]
        public float ExplosionChance = .1f;

        /// <summary>
        ///     Chance of lightning occurring when we microwave a metallic object
        /// </summary>
        [DataField]
        public float LightningChance = .75f;
    }

    public sealed class BeingMicrowavedEvent : HandledEntityEventArgs
    {
        public EntityUid Microwave;
        public EntityUid? User;

        public BeingMicrowavedEvent(EntityUid microwave, EntityUid? user)
        {
            Microwave = microwave;
            User = user;
        }
    }
}
