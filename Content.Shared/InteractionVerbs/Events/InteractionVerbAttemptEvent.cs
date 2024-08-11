namespace Content.Shared.InteractionVerbs.Events;

/// <summary>
///     Raised directly on the performer of the interaction verb and on its target to determine if it should be allowed.
///     Note that this is raised if and only if verb's own CanPerform check returns true.
/// </summary>
[ByRefEvent]
public sealed class InteractionVerbAttemptEvent(InteractionVerbPrototype proto, EntityUid user, EntityUid target) : CancellableEntityEventArgs
{
    public InteractionVerbPrototype Proto => proto;
    public EntityUid User => user;
    public EntityUid Target => target;
}
