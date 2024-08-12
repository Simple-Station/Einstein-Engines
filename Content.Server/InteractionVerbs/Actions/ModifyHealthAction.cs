using Content.Shared.Damage;
using Content.Shared.InteractionVerbs;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ModifyHealthAction : InteractionAction
{
    [DataField(required: true)] public DamageSpecifier Damage = default!;
    [DataField] public bool IgnoreResistance = false;

    public override bool IsAllowed(EntityUid user, EntityUid target, InteractionVerbPrototype proto, bool canAccess, bool canInteract, VerbDependencies deps)
    {
        return deps.EntMan.HasComponent<DamageableComponent>(target);
    }

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        // TODO: check if container supports this kind of damage?
        return true;
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        deps.EntMan.System<DamageableSystem>()
            .TryChangeDamage(target, Damage, IgnoreResistance, origin: user);
    }
}
