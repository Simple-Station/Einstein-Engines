namespace Content.Shared.Body.Events;

[ByRefEvent]
public record struct SuffocationDamageEvent(EntityUid Uid)
{
    public readonly EntityUid Uid = Uid;
}
