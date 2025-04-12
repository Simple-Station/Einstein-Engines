namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Veil Ability
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingVeilComponent : Component
{
    public string? ActionGlare = "ActionVeil";

    [DataField]
    public float Range = 8f;
}
