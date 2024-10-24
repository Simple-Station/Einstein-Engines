using Content.Shared.Construction.Prototypes;
using Content.Shared.MedicalScanner;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Medical.Components
{
    [RegisterComponent]
    public sealed partial class MedicalScannerComponent : SharedMedicalScannerComponent
    {
        public const string ScannerPort = "MedicalScannerReceiver";
        public ContainerSlot BodyContainer = default!;
        public EntityUid? ConnectedConsole;

        [ViewVariables(VVAccess.ReadWrite)]
        public float CloningFailChanceMultiplier = 1f;

        public float MetemKarmaBonus = 0.25f;

        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartCloningFailChance = "Capacitor";

        [DataField]
        public float PartRatingFailMultiplier = 0.75f;
    }
}
