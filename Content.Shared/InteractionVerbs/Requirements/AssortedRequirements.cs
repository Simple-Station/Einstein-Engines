using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;

namespace Content.Shared.InteractionVerbs.Requirements;

/// <summary>
///     Requires the target to meet a certain whitelist and not meet a blacklist.
/// </summary>
public sealed partial class EntityWhitelistRequirement : InteractionRequirement
{
    [DataField] public EntityWhitelist Whitelist = new(), Blacklist = new();

    public override bool IsMet(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, InteractionVerbAction.VerbDependencies deps)
    {
        return Whitelist.IsValid(target, deps.EntMan) && !Blacklist.IsValid(target, deps.EntMan);
    }
}

/// <summary>
///     Requires the mob to be a mob in a certain state. If inverted, requires the mob to not be in that state.
/// </summary>
public sealed partial class MobStateRequirement : InvertableInteractionRequirement
{
    [DataField] public List<MobState> AllowedStates = new();

    public override bool IsMet(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, InteractionVerbAction.VerbDependencies deps)
    {
        if (!deps.EntMan.TryGetComponent<MobStateComponent>(target, out var state))
            return false;

        return AllowedStates.Contains(state.CurrentState) ^ Inverted;
    }
}

/// <summary>
///     Requires the target to be in a specific standing state.
/// </summary>
public sealed partial class StandingStateRequirement : InteractionRequirement
{
    [DataField] public bool AllowStanding, AllowLaying, AllowKnockedDown;

    public override bool IsMet(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, InteractionVerbAction.VerbDependencies deps)
    {
        if (deps.EntMan.HasComponent<KnockedDownComponent>(target))
            return AllowKnockedDown;

        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(target, out var state))
            return false;

        return state.Standing ? AllowStanding : AllowLaying;
    }
}

/// <summary>
///     Requires the target to be the user itself.
/// </summary>
public sealed partial class SelfTargetRequirement : InvertableInteractionRequirement
{
    public override bool IsMet(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, InteractionVerbAction.VerbDependencies deps)
    {
        return (user == target) ^ Inverted;
    }
}
