namespace Content.Shared.Body.Events;

[ByRefEvent]
public record struct SuffocationSoundEvent(EntityUid Uid)
{
    public readonly EntityUid Uid = Uid;
}
