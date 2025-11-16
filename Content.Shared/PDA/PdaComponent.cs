using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.PDA
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class PdaComponent : Component
    {
        public const string PdaIdSlotId = "PDA-id";
        public const string PdaPenSlotId = "PDA-pen";
        public const string PdaPaiSlotId = "PDA-pai";
        public const string PdaPassportSlotId = "PDA-passport";
        public const string PdaShipDeedSlotId = "PDA-shipdeed";
        public const string PdaManual1SlotId = "PDA-manual1";
        public const string PdaManual2SlotId = "PDA-manual2";
        public const string PdaManual3SlotId = "PDA-manual3";

        /// <summary>
        /// The base PDA sprite state, eg. "pda", "pda-clown"
        /// </summary>
        [DataField("state")]
        public string? State;

        [DataField]
        public ItemSlot IdSlot = new();

        [DataField]
        public ItemSlot PenSlot = new();
        [DataField]
        public ItemSlot PaiSlot = new();
        [DataField]
        public ItemSlot PassportSlot = new();
        [DataField]
        public ItemSlot ShipDeedSlot = new();
        [DataField]
        public ItemSlot Manual1Slot = new();
        [DataField]
        public ItemSlot Manual2Slot = new();
        [DataField]
        public ItemSlot Manual3Slot = new();

        // Really this should just be using ItemSlot.StartingItem. However, seeing as we have so many different starting
        // PDA's and no nice way to inherit the other fields from the ItemSlot data definition, this makes the yaml much
        // nicer to read.
        [DataField("id", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? IdCard;

        [ViewVariables] public EntityUid? ContainedId;
        [ViewVariables] public bool FlashlightOn;

        [ViewVariables(VVAccess.ReadWrite)] public string? OwnerName;
        [ViewVariables] public string? StationName;
        [ViewVariables] public string? StationAlertLevel;
        [ViewVariables] public Color StationAlertColor = Color.White;
    }
}
