using Content.Server.Traits;
using Content.Shared.Roles;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Jobs;


[UsedImplicitly]
public sealed partial class AddTraitSpecial : JobSpecial
{
    // Datafield for storing multiple trait prototype IDs as strings
    [DataField(required: true)]
    public HashSet<string> Traits { get; private set; } = new();

    public override void AfterEquip(EntityUid mob)
    {
        // Resolve the necessary systems
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var traitSystem = entityManager.System<TraitSystem>();

        // Iterate through each trait and add it to the entity
        foreach (var traitId in Traits)
            if (prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))

                traitSystem.AddTrait(mob, traitPrototype);
    }
}
