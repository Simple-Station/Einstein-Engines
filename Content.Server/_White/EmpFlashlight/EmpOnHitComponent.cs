using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;
using Content.Server._White.EmpFlashlight;
using Content.Server.Emp;

namespace Content.Server._White.EmpFlashlight;

/// <summary>
/// Upon being triggered will EMP target.
/// </summary>
[RegisterComponent]
[Access(typeof(EmpOnHitSystem))]

public sealed partial class EmpOnHitComponent: Component
{
    [DataField("range"), ViewVariables(VVAccess.ReadWrite)]
    public float Range = 1.0f;

    [DataField("energyConsumption"), ViewVariables(VVAccess.ReadWrite)]
    public float EnergyConsumption;

    [DataField("disableDuration"), ViewVariables(VVAccess.ReadWrite)]
    public float DisableDuration = 60f;
}
