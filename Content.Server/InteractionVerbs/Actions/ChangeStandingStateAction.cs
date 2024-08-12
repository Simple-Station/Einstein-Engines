using Content.Shared.InteractionVerbs;
using Content.Shared.Standing;

namespace Content.Server.InteractionVerbs.Actions;

public sealed partial class ChangeStandingStateAction : InteractionVerbAction
{
    [DataField]
    public bool MakeStanding, MakeLaying;

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(target, out var state))
            return false;

        return state.Standing ? MakeStanding : MakeLaying;
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var stateSystem = deps.EntMan.System<StandingStateSystem>();
        var isDown = stateSystem.IsDown(target);

        // Note: these will get cancelled if the target is forced to stand/lay, e.g. due to a buckle or stun or something else.
        if (isDown && MakeStanding)
            stateSystem.Stand(target);
        else if (!isDown && MakeLaying)
            stateSystem.Down(target);
    }
}
