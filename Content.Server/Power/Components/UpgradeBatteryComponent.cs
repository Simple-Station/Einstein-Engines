using Content.Shared.Construction.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Power.Components
{

    [RegisterComponent]
    public sealed partial class UpgradeBatteryComponent : Component
    {
        /// <summary>
        ///     The machine part that affects the power capacity.
        /// </summary>
        [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartPowerCapacity = "PowerCell";

        /// <summary>
        ///     The machine part rating is raised to this power when calculating power gain
        /// </summary>
        [DataField]
        public float MaxChargeMultiplier = 2f;

        /// <summary>
        ///     Power gain scaling
        /// </summary>
        [DataField]
        public float BaseMaxCharge = 8000000;
    }
}
