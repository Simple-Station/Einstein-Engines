using Robust.Shared.GameStates;

namespace Content.Shared._EE.Shadowling;

/// <summary>
/// This is used for the Basic Enthrall Ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingEnthrallComponent : Component
{
    public string? GlareAction { get; set; } = "ActionEnthrall";

    [DataField]
    public TimeSpan EnthrallTime = TimeSpan.FromSeconds(1.5); // this needs actual playtesting but should be fine for now
}
