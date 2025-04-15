using Robust.Shared.GameStates;

namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Blindness Smoke ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlindnessSmokeComponent : Component
{
    public string? ActionBlindnessSmoke = "ActionBlindnessSmoke";
}
