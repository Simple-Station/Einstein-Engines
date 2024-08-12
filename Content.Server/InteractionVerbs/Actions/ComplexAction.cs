using System.Linq;
using Content.Shared.InteractionVerbs;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     An action that combines multiple other actions.
/// </summary>
public sealed partial class ComplexAction : InteractionVerbAction
{
    [DataField]
    public List<InteractionVerbAction> Actions = new();

    /// <summary>
    ///     If true, all actions must pass the IsAllowed and CanPerform checks (boolean and). Otherwise, at least one must pass (boolean or).
    /// </summary>
    [DataField]
    public bool RequireAll = false;

    public override bool IsAllowed(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, VerbDependencies deps)
    {
        return RequireAll
            ? Actions.All(a => a.IsAllowed(user, target, proto, canAccess, canInteract, deps))
            : Actions.Any(a => a.IsAllowed(user, target, proto, canAccess, canInteract, deps));
    }

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return RequireAll
            ? Actions.All(a => a.CanPerform(user, target, beforeDelay, proto, deps))
            : Actions.Any(a => a.CanPerform(user, target, beforeDelay, proto, deps));
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        foreach (var action in Actions)
        {
            action.Perform(user, target, proto, deps);
        }
    }
}
