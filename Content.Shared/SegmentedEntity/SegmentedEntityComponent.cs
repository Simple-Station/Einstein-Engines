using Robust.Shared.GameStates;

namespace Content.Shared.SegmentedEntity;

/// <summary>
///     Controls initialization of any Multi-segmented entity
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SegmentedEntityComponent : Component
{
    /// <summary>
    ///     A list of each UID attached to the Lamia, in order of spawn
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<NetEntity> Segments = new();

    /// <summary>
    ///     A clamped variable that represents the number of segments to be spawned
    /// </summary>
    [DataField]
    public int NumberOfSegments = 18;

    /// <summary>
    ///     How wide the initial segment should be.
    /// </summary>
    [DataField]
    public float InitialRadius = 0.3f;

    /// <summary>
    ///     Texture of the segment.
    /// </summary>
    [DataField(required: true)]
    public string TexturePath;

    /// <summary>
    ///     If UseTaperSystem is true, this constant represents the rate at which a segmented entity will taper towards the tip. Tapering is on a logarithmic scale, and will asymptotically approach 0.
    /// </summary>
    [DataField]
    public float OffsetConstant = 1.03f;

    /// <summary>
    ///     Represents the prototype used to parent all segments
    /// </summary>
    [DataField]
    public string InitialSegmentId = "LamiaInitialSegment";

    /// <summary>
    ///     Represents the segment prototype to be spawned
    /// </summary>
    [DataField]
    public string SegmentId = "LamiaSegment";

    /// <summary>
    ///     How much to slim each successive segment.
    /// </summary>
    [DataField]
    public float SlimFactor = 0.93f;

    /// <summary>
    ///     Set to false for constant width
    /// </summary>
    [DataField]
    public bool UseTaperSystem = true;

    /// <summary>
    ///     The standard distance between the centerpoint of each segment.
    /// </summary>
    [DataField]
    public float StaticOffset = 0.15f;

    /// <summary>
    ///     The standard sprite scale of each segment.
    /// </summary>
    [DataField]
    public float StaticScale = 1f;

    /// <summary>
    ///     Used to more finely tune how much damage should be transfered from tail to body.
    /// </summary>
    [DataField]
    public float DamageModifierOffset = 0.4f;

    /// <summary>
    ///     A clamped variable that represents how far from the tip should tapering begin.
    /// </summary>
    [DataField]
    public int TaperOffset = 18;

    /// <summary>
    ///     Coefficient used to finely tune how much explosion damage should be transfered to the body. This is calculated multiplicatively with the derived damage modifier set.
    /// </summary>
    [DataField]
    public float ExplosiveModifierOffset = 0.1f;

    /// <summary>
    ///     Controls whether or not lamia segments always block bullets, or use the bullet passover system for laying down bodies.
    /// </summary>
    [DataField]
    public bool BulletPassover = true;
}
