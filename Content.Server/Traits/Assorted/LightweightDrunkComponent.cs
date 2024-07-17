namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
/// Used for the lightweight trait. LightweightDrunkSystem will multiply the effects of ethanol being metabolized
/// </summary>
[RegisterComponent]
public sealed partial class LightweightDrunkComponent : Component
{
    [DataField("boozeStrengthMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float BoozeStrengthMultiplier = 4f;
}
