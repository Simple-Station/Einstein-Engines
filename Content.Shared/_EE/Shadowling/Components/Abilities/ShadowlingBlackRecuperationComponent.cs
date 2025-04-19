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
    public DamageSpecifier Healing = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            ["Blunt"] = -33,
            ["Slash"] = -33,
            ["Piercing"] = -33,
            ["Heat"] = -33,
            ["Cold"] = -33,
            ["Shock"] = -33,
            ["Asphyxiation"] = -100,
            ["Bloodloss"] = -100,
            ["Poison"] = -50,
            ["Cellular"] = -50
        }
    };
}
