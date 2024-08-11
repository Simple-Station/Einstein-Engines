using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Random;

namespace Content.Shared.InteractionVerbs.Actions;

/// <summary>
///     Requires the target to be a mob in a specific state.
///     Does nothing on its own, made just to mimic the old "chance to show a popup" interactions.
/// </summary>
public partial class MobNoOpAction : NoOpAction
{
    [DataField]
    public MobState? RequiredState = MobState.Alive;

    public override bool IsAllowed(EntityUid user, EntityUid target, InteractionVerbPrototype _, bool __, bool ___, VerbDependencies deps)
    {
        return deps.EntMan.TryGetComponent<MobStateComponent>(target, out var state)
               && (RequiredState == null || state.CurrentState == RequiredState);
    }
}
