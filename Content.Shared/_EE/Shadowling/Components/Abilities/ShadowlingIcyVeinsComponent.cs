using Content.Shared.Atmos;
using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Icy Veins.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingIcyVeinsComponent : Component
{
    public string? ActionIcyVeins = "ActionIcyVeins";

    [DataField]
    public float Range = 6f;

    [DataField]
    public float ParalyzeTime = 1f;
}
