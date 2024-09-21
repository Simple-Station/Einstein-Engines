using Content.Shared.Atmos;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Atmos.Piping.Binary.Components
{
    [RegisterComponent]
    public sealed partial class GasRecyclerComponent : Component
    {
        [DataField("reacting")]
        public Boolean Reacting { get; set; } = false;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("inlet")]
        public string InletName { get; set; } = "inlet";

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("outlet")]
        public string OutletName { get; set; } = "outlet";

        [DataField]
        public float MinTemp = 300 + Atmospherics.T0C;

        [DataField]
        public float BaseMinTemp = 300 + Atmospherics.T0C;

        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartMinTemp = "Capacitor";

        [DataField]
        public float PartRatingMinTempMultiplier = 0.95f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float MinPressure = 30 * Atmospherics.OneAtmosphere;

        [DataField]
        public float BaseMinPressure = 30 * Atmospherics.OneAtmosphere;

        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartMinPressure = "Manipulator";

        [DataField]
        public float PartRatingMinPressureMultiplier = 0.8f;
    }
}
