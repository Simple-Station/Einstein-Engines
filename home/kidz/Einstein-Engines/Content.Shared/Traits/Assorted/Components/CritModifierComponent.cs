using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     This is used for any trait that modifies CritThreshold
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CritModifierComponent : Component
{
    /// <summary>
    ///     The amount that an entity's critical threshold will be incremented by.
    /// </summary>
    [DataField]
    public int CritThresholdModifier { get; private set; } = 0;
}