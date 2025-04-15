namespace Content.Shared._Crescent.ShipBalanceEnforcement;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class ShipSpeedByMassAdjusterComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float InitialGridMass = 0f;
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float InitialGridSpeed = 0f;
}
