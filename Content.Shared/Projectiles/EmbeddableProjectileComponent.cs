using Content.Shared._Shitmed.Targeting;
using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.GameStates;

namespace Content.Shared.Projectiles;

/// <summary>
/// Embeds this entity inside of the hit target.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmbeddableProjectileComponent : Component
{
    /// <summary>
    /// Minimum speed of the projectile to embed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MinimumSpeed = 5f;

    /// <summary>
    /// Delete the entity on embedded removal?
    /// Does nothing if there's no RemovalTime.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DeleteOnRemove;

    /// <summary>
    /// How long it takes to remove the embedded object.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? RemovalTime = 5f;

    /// <summary>
    ///     Whether this entity will embed when thrown, or only when shot as a projectile.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool EmbedOnThrow = true;

    /// <summary>
    /// How far into the entity should we offset (0 is wherever we collided).
    /// </summary>
    [DataField, AutoNetworkedField]
    public Vector2 Offset = Vector2.Zero;

    /// <summary>
    /// Sound to play after embedding into a hit target.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound;

    /// <summary>
    /// Uid of the entity the projectile is embed into.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? EmbeddedIntoUid;

    /// <summary>
    ///   How much time before this entity automatically falls off? (0 is never)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AutoRemoveDuration = 40f;

    /// <summary>
    ///   The time when this entity automatically falls off after being attached.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan? AutoRemoveTime = null;

    /// <summary>
    ///   The body part of the target this embeddable is attached to.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public TargetBodyPart? TargetBodyPart = null;
}
