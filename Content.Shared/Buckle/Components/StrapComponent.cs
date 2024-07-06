using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Buckle.Components;

/// <summary>
/// Component that allows entities to be buckled to it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedBuckleSystem))]
public sealed partial class StrapComponent : Component
{
    /// <summary>
    /// The entities that are currently buckled to this strap.
    /// </summary>
    [ViewVariables]
    public HashSet<EntityUid> BuckledEntities = new();

    /// <summary>
    /// The maximum distance from which entities can buckle to this strap.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float MaxBuckleDistance = 1.0f;

    /// <summary>
    /// The position that entities will be in when buckled to this strap.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public StrapPosition Position = StrapPosition.Stand;

    /// <summary>
    /// The offset from the strap's position where entities will be buckled.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 BuckleOffset = Vector2.Zero;

    /// <summary>
    /// The maximum size of entities that can be buckled to this strap.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Size = 100;

    /// <summary>
    /// The current occupied size of this strap.
    /// </summary>
    [ViewVariables]
    public int OccupiedSize;

    /// <summary>
    /// Whether this strap is enabled or not.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled = true;

    /// <summary>
    /// The alert type to show when an entity is buckled to this strap.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public AlertType BuckledAlertType = AlertType.Buckled;

    /// <summary>
    /// The sound to play when an entity is buckled to this strap.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier BuckleSound = new SoundPathSpecifier("/Audio/Effects/buckle.ogg");

    /// <summary>
    /// The sound to play when an entity is unbuckled from this strap.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier UnbuckleSound = new SoundPathSpecifier("/Audio/Effects/unbuckle.ogg");
}

public enum StrapPosition
{
    None,
    Stand,
    Down
}
