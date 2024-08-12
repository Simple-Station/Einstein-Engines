using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs;

/// <summary>
///     Defines a requirement for an <see cref="InteractionVerb"/>.
///     If a verb does not meet the requirement, it will be hidden or disabled in the verb menu.
/// </summary>
[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors )]
public abstract partial class InteractionRequirement
{
    public abstract bool IsMet(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, InteractionAction.VerbDependencies deps);
}

// <inheritdoc/>
[Serializable, NetSerializable]
public abstract partial class InvertableInteractionRequirement : InteractionRequirement
{
    [DataField] public bool Inverted = false;
}
