using Content.Shared.Interaction;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Buckle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedBuckleSystem))]
public sealed partial class BuckleComponent : Component
{
    /// <summary>
    /// The range from which this entity can buckle to a <see cref="StrapComponent"/>.
    /// Separated from normal interaction range to fix the "someone buckled to a strap
    /// across a table two tiles away" problem.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Range = SharedInteractionSystem.InteractionRange / 1.4f;

    /// <summary>
    /// True if the entity is buckled, false otherwise.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool Buckled;

    /// <summary>
    /// The last entity this component was buckled to.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public EntityUid? LastEntityBuckledTo;

    /// <summary>
    /// Whether or not collisions should be possible with the entity we are strapped to.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public bool DontCollide;

    /// <summary>
    /// Whether or not we should be allowed to pull the entity we are strapped to.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public bool PullStrap;

    /// <summary>
    /// The delay before the buckle/unbuckle action is completed.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.25f);

    /// <summary>
    /// The time when the buckle/unbuckle action was initiated.
    /// </summary>
    [ViewVariables]
    public TimeSpan BuckleTime;

    /// <summary>
    /// The entity this component is currently buckled to.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public EntityUid? BuckledTo;

    /// <summary>
    /// The maximum size of entities that can be buckled to this component.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Size = 100;

    /// <summary>
    /// The original draw depth of the entity before it was buckled.
    /// </summary>
    [ViewVariables] public int? OriginalDrawDepth;

    [DataField]

    [ViewVariables(VVAccess.ReadWrite)]

    public EntityWhitelist? AllowedBuckleTypes;
}

[ByRefEvent]
public record struct BuckleAttemptEvent(EntityUid StrapEntity, EntityUid BuckledEntity, EntityUid UserEntity, bool Buckling, bool Cancelled = false);

[ByRefEvent]
public readonly record struct BuckleChangeEvent(EntityUid StrapEntity, EntityUid BuckledEntity, bool Buckling);

[Serializable, NetSerializable]
public enum BuckleVisuals
{
    Buckled
}

