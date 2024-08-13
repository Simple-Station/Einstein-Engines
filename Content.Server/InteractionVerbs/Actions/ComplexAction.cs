using System.Linq;
using Content.Shared.InteractionVerbs;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     An action that combines multiple other actions.
/// </summary>
[Serializable]
public sealed partial class ComplexAction : InteractionAction
{
    [DataField]
    public List<InteractionAction> Actions = new();

    /// <summary>
    ///     If true, all actions must pass the IsAllowed and CanPerform checks,
    ///     and all must successfully perform for this action to succeed (boolean and).
    ///     Otherwise, at least one must pass the checks and successfully perform (boolean or).
    /// </summary>
    /// <remarks>If this is false, all actions will be performed if at least one of their CanPerform checks succeeds.</remarks>
    [DataField]
    public bool RequireAll = false;

    private bool Delegate(Func<InteractionAction, bool> delegatedAction)
    {
        return RequireAll ? Actions.All(delegatedAction) : Actions.Any(delegatedAction);
    }

    public override bool IsAllowed(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return Delegate(act => act.IsAllowed(args, proto, deps));
    }

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool beforeDelay, VerbDependencies deps)
    {
        return Delegate(act => act.CanPerform(args, proto, beforeDelay, deps));
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return Delegate(act => act.Perform(args, proto, deps));
    }
}
