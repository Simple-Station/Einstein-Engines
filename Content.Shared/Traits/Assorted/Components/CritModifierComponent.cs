using Robust.Shared.GameStates;
//using Content.Shared.FixedPoint;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     This is used for any trait that modifies CritThreshold
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CritModifierComponent : Component
{
    [DataField] public float CritThresholdModifier { get; set; } = 0f;
    [ViewVariables] public float OriginalCritThreshold { get; set; } = 0f;
    [ViewVariables] public float ChemActive { get; set; } = 0f;
}
