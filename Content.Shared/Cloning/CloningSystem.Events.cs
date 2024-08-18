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
///     Raised on a corpse being subjected to forced reincarnation(Metempsychosis). Allowing for innate effects from the mob to influence the reincarnation.
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
