using Content.Shared.InteractionVerbs;
using Content.Shared.Standing;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ChangeStandingStateAction : InteractionAction
{
    [DataField]
    public bool MakeStanding, MakeLaying;

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(target, out var state))
            return false;

        return state.Standing ? MakeLaying : MakeStanding;
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
