namespace Content.Shared.Cloning;

/// <summary>
///     Raised after a new mob got spawned when cloning a humanoid
/// </summary>
[ByRefEvent]
public struct CloningEvent
{
    public bool NameHandled = false;

    public readonly EntityUid Source;
    public readonly EntityUid Target;

    public CloningEvent(EntityUid source, EntityUid target)
    {
        Source = source;
        Target = target;
    }
}

/// <summary>
///     Raised on a corpse being subjected to forced reincarnation(Metempsychosis).
///     Allowing for innate effects from the mob to influence the reincarnation.
/// </summary>
[ByRefEvent]
public struct ReincarnatingEvent
{
    public bool OverrideChance;
    public bool NeverTrulyClone;
    public ForcedMetempsychosisType ForcedType = ForcedMetempsychosisType.None;
    public readonly EntityUid OldBody;
    public float ReincarnationChanceModifier = 1;
    public float ReincarnationChances;
    public ReincarnatingEvent(EntityUid oldBody, float reincarnationChances)
    {
        OldBody = oldBody;
        ReincarnationChances = reincarnationChances;
    }
}

/// <summary>
///     Raised on a corpse that someone is attempting to clone, but before the process actually begins.
///     Allows for Entities to influence whether the cloning can begin in the first place, either by canceling it, or modifying the cost.
/// </summary>
[ByRefEvent]
public struct AttemptCloningEvent
{
    public bool Cancelled;
    public bool DoMetempsychosis;
    public EntityUid CloningPod;
    public string? CloningFailMessage;
    public float CloningCostMultiplier = 1;
    public AttemptCloningEvent(EntityUid cloningPod, bool doMetempsychosis)
    {
        DoMetempsychosis = doMetempsychosis;
        CloningPod = cloningPod;
    }
}
