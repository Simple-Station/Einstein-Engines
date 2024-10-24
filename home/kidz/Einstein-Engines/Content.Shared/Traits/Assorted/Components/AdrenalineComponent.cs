using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     This is used for any trait that modifies the Melee System implementation of Health Contest
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AdrenalineComponent : Component
{
    /// <summary>
    ///     When true, multiplies by the inverse of the resulting Contest.
    /// </summary>
    [DataField]
    public bool Inverse { get; private set; } = false;

    /// <summary>
    ///     Used as the RangeModifier input for a Health Contest.
    /// </summary>
    [DataField]
    public float RangeModifier { get; private set; } = 1;

    /// <summary>
    ///     Used as the BypassClamp input for a Health Contest.
    /// </summary>
    [DataField]
    public bool BypassClamp { get; private set; } = false;
}