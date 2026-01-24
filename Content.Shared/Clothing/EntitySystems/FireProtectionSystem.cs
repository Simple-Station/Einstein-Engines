using Content.Goobstation.Common.Flammability;
using Content.Shared.Atmos;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// Handles reducing fire damage when wearing clothing with <see cref="FireProtectionComponent"/>.
/// </summary>
public sealed class FireProtectionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireProtectionComponent, InventoryRelayedEvent<GetFireProtectionEvent>>(OnGetProtection);
    }

    private void OnGetProtection(Entity<FireProtectionComponent> ent, ref InventoryRelayedEvent<GetFireProtectionEvent> args)
    {
        // goob edit - VERY flammable component (trademark)
        if (HasComp<VeryFlammableComponent>(ent))
            return;

        args.Args.Reduce(ent.Comp.Reduction);
    }
}
