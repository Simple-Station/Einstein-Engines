using Robust.Shared.GameStates;

namespace Content.Shared.SegmentedEntity;

/// <summary>
///     This is a tracking component used for storing quick reference information related to a Lamia's main body on each of her segments,
///     which is needed both for simplifying a lot of code, as well as tracking who the original body was.
///     None of these are Datafields for a reason, they are modified by the original parent body upon spawning her segments.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SegmentedEntitySegmentComponent : Component
{
    [ViewVariables]
    public EntityUid AttachedToUid = default!;

    [ViewVariables]
    public float DamageModifyFactor = default!;

    [ViewVariables]
    public float OffsetSwitching = default!;

    [ViewVariables]
    public float ScaleFactor = default!;

    [ViewVariables]
    public float DamageModifierCoefficient = default!;

    [ViewVariables]
    public float ExplosiveModifyFactor = default!;

    [ViewVariables]
    public float OffsetConstant = default!;

    [ViewVariables]
    public EntityUid Lamia = default!;

    [ViewVariables]
    public int MaxSegments = default!;

    [ViewVariables]
    public int SegmentNumber = default!;

    [ViewVariables]
    public float DamageModifierConstant = default!;

    [ViewVariables]
    public string? SegmentId;
}
