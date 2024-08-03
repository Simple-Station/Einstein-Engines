using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     This is used for any trait that modifies footstep volumes.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootstepVolumeModifierComponent : Component
{
    /// <summary>
    ///     What to add to the volume of sprinting, in terms of decibels.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SprintingModifier { get; set; } = 0f;

    /// <summary>
    ///     What to add to the volume of walking, in terms of decibels.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WalkingModifier { get; set; } = 0f;

    /// <summary>
    ///     What to multiply the distance needed to hear sprinting by.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SprintDistanceMultiplier { get; set; } = 1f;

    /// <summary>
    ///     What to multiply the distance needed to hear walking by.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WalkDistanceMultiplier { get; set; } = 1f;
}
