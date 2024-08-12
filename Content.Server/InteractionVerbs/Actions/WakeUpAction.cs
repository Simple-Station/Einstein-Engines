using Content.Server.Bed.Sleep;
using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.InteractionVerbs;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class WakeUpAction : InteractionAction
{
    public override bool IsAllowed(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, VerbDependencies deps)
    {
        return deps.EntMan.HasComponent<SleepingComponent>(target);
    }

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return deps.EntMan.HasComponent<SleepingComponent>(target);
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        deps.EntMan.System<SleepingSystem>().TryWaking(target, user: user);
    }
}
