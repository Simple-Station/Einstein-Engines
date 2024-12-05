namespace Content.Server.EmpFlashlight;

/// <summary>
///     Upon being triggered will EMP target.
/// </summary>
[RegisterComponent]
[Access(typeof(EmpOnHitSystem))]

public sealed partial class EmpOnHitComponent : Component
{
    [DataField]
    public float Range = 1.0f;

    [DataField]
    public float EnergyConsumption;

    [DataField]
    public float DisableDuration = 60f;
}
