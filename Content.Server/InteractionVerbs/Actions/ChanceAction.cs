using Content.Shared.InteractionVerbs;
using Robust.Shared.Random;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     Gives another action a probability to be run.
/// </summary>
public sealed partial class ChanceAction : InteractionVerbAction
{
    [DataField(required: true)]
    public InteractionVerbAction Action = default!;

    [DataField(required: true)]
    public float Chance;

    public override bool IsAllowed(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, VerbDependencies deps)
    {
        return Action.IsAllowed(user, target, proto, canAccess, canInteract, deps);
    }

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (beforeDelay)
            return true; // We only roll after the do-after ends, if any

        var result = Chance > 0f && Chance < 1f || deps.Random.Prob(Chance);
        return result && Action.CanPerform(user, target, beforeDelay, proto, deps);
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        Action.Perform(user, target, proto, deps);
    }
}
