using Content.Shared.InteractionVerbs;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     An action that performs one of the two (or just one) actions based on a condition.
/// </summary>
[Serializable]
public sealed partial class ConditionalAction : InteractionAction
{
    [DataField(required: true)]
    public InteractionRequirement Condition;

    [DataField("true")]
    public InteractionAction? TrueBranch;

    [DataField("false")]
    public InteractionAction? FalseBranch;

    /// <summary>
    ///     If true, the CanPerform() check will fail when the condition results in a <c>null</c> branch.
    ///     Otherwise, null branch is equivalent to a no-op action.
    /// </summary>
    [DataField("failWhenNull")]
    public bool FailWhenNoBranch = false;

    /// <summary>
    ///     If true, the IsValid() check will be delegated to the respective branch.
    ///     If the respective branch is <c>null</c>, the decision will be made based on <see cref="FailWhenNoBranch"/>
    /// </summary>
    [DataField("delegateValid")]
    public bool DelegateValidation = false;

    public override bool IsAllowed(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, VerbDependencies deps)
    {
        if (!DelegateValidation)
            return true;

        var branch = Condition.IsMet(user, target, proto, canAccess, canInteract, deps) ? TrueBranch : FalseBranch;
        return branch?.IsAllowed(user, target, proto, canAccess, canInteract, deps) ?? !FailWhenNoBranch;
    }

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (beforeDelay)
            return true;

        // TODO: we assume the user can interact and access the target here
        var branch = Condition.IsMet(user, target, proto, true, true, deps) ? TrueBranch : FalseBranch;
        return branch?.CanPerform(user, target, beforeDelay, proto, deps) ?? !FailWhenNoBranch;
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var branch = Condition.IsMet(user, target, proto, true, true, deps) ? TrueBranch : FalseBranch;
        branch?.Perform(user, target, proto, deps);
    }
}
