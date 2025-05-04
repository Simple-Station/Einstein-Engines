namespace Content.Server._White.Blocking;

[RegisterComponent]
public sealed partial class RechargeableBlockingComponent : Component
{
    [DataField]
    public float DischargedRechargeRate = 1.33f;

    [DataField]
    public float ChargedRechargeRate = 2f;

    [ViewVariables]
    public bool Discharged;
}
