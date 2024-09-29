using Robust.Shared.GameStates;

namespace Content.Server.Traits.Assorted;

/// <summary>
///     This is used for traits that modify the outcome of Potentia Rolls
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PotentiaModifierComponent : Component
{
    /// <summary>
    ///     When rolling for psionic powers, increase the potentia gains by a flat amount.
    /// </summary>
    [DataField]
    public float PotentiaFlatModifier { get; private set; }

    /// <summary>
    ///     When rolling for psionic powers, multiply the potentia gains by a specific factor.
    /// </summary>
    [DataField]
    public float PotentiaMultiplier { get; private set; }
}