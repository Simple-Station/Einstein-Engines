using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs.Requirements;

/// <summary>
///     A requirement that combines multiple other requirements.
/// </summawry>
[Serializable, NetSerializable]
public sealed partial class ComplexRequirement : InteractionRequirement
{
    [DataField]
    public List<InteractionRequirement> Requirements = new();

    /// <summary>
    ///     If true, all requirements must pass (boolean and). Otherwise, at least one must pass (boolean or).
    /// </summary>
    [DataField]
    public bool RequireAll = false;

    public override bool IsMet(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, InteractionAction.VerbDependencies deps)
    {
        return RequireAll
            ? Requirements.All(r => r.IsMet(user, target, proto, canAccess, canInteract, deps))
            : Requirements.Any(r => r.IsMet(user, target, proto, canAccess, canInteract, deps));
    }
}
