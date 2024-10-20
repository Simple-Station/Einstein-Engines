using Content.Server.Construction.Components;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Power.Components;

[RegisterComponent]
public sealed partial class UpgradePowerSupplyRampingComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float BaseRampRate;

    /// <summary>
    ///     The machine part that affects the power supply ramping
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartRampRate = "Capacitor";

    /// <summary>
    ///     The multiplier used for scaling the power supply ramping
    /// </summary>
    [DataField]
    public float SupplyRampingMultiplier = 1f;

    /// <summary>
    ///     What type of scaling is being used?
    /// </summary>
    [DataField(required: true)]
    public MachineUpgradeScalingType Scaling;

    /// <summary>
    ///     The current value that the power supply is being scaled by
    /// </summary>
    [DataField]
    public float ActualScalar = 1f;
}
