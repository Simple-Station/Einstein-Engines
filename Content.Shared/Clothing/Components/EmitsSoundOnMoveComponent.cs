using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared.Clothing.Components;

/// <summary>
///   Indicates that the clothing entity emits sound when it moves.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmitsSoundOnMoveComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField(required: true), AutoNetworkedField]
    public SoundSpecifier SoundCollection = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("requiresGravity"), AutoNetworkedField]
    public bool RequiresGravity = true;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates LastPosition = EntityCoordinates.Invalid;

    /// <summary>
    ///   The distance moved since the played sound.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float SoundDistance = 0f;

    /// <summary>
    ///   Whether this item is equipped in a inventory item slot.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsSlotValid = true;

    /// <summary>
    ///     If worn, how far the wearer has to walk in order to make a sound.
    /// </summary>
    [DataField]
    public float DistanceWalking = 1.5f;

    /// <summary>
    ///     If worn, how far the wearer has to sprint in order to make a sound.
    /// </summary>
    [DataField]
    public float DistanceSprinting = 2f;

    /// <summary>
    ///     Whether or not this item must be worn in order to make sounds.
    /// </summary>
    [DataField]
    public bool RequiresWorn;
}
