namespace Content.Shared._Crescent.DegradeableArmor;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class ArmorRepairKitComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float repairHealth = 500f;
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ArmorRepairMaterial materialType = ArmorRepairMaterial.PlasteelPlate;
}
