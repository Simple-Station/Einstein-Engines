using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     A component that intensifies moodlets by a random amount.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ManicComponent : Component
{
    /// <summary>
    ///     The lower bound for multiplying moodlet effects. This also deadens negative moods.
    /// </summary>
    [DataField]
    public float LowerMultiplier = 0.7f;

    /// <summary>
    ///     The amount to multiply moodlets by. This will also intensify negative moods too.
    /// </summary>
    [DataField]
    public float UpperMultiplier = 1.3f;
}
