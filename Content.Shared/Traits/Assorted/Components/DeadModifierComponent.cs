using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     This is used for any trait that modifies DeadThreshold
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DeadModifierComponent : Component
{
    /// <summary>
    ///     The amount that an entity's DeadThreshold will be incremented by.
    /// </summary>
    [DataField]
    public int DeadThresholdModifier { get; private set; } = 0;
}