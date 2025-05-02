using Content.Shared.Damage;
using Content.Shared.FixedPoint;

using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Black Recuperation ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlackRecuperationComponent : Component
{
    [DataField]
    public bool IsEmpowering;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    [DataField]
    public string? BlackRecuperationEffect = "ShadowlingBlackRecuperationEffect";
}

