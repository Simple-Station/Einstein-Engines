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
    public float SprintVolumeModifier;

    /// <summary>
    ///     What to add to the volume of walking, in terms of decibels.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WalkVolumeModifier;
}
