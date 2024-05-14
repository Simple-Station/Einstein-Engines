using System.Linq;
using Content.Shared.Aliens.Components;
using Content.Shared.Ghost;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Shared.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedFacehuggerSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FacehuggerComponent>();

        var validEntities = new Dictionary<EntityUid, EntityUid>();

        while (query.MoveNext(out var uid, out var alien))
        {
            foreach (var entity in _lookup.GetEntitiesInRange(uid, alien.Range)
                         .Where(entity => _inventory.HasSlot(entity, "mask")))
            {
                if (!_inventory.CanAccess(uid, entity, uid) ||
                    EnsureComp<MobStateComponent>(uid).CurrentState != MobState.Alive)
                    continue;
                if(Prototype(entity) != null && Prototype(entity)!.ID == "AdminObserver")
                    continue;
                if(!HasComp<MobStateComponent>(entity) || Comp<MobStateComponent>(entity).CurrentState == MobState.Dead)
                    continue;
                if(HasComp<AlienInfectedComponent>(entity))
                    continue;
                if(!alien.Active)
                    continue;
                validEntities.TryAdd(uid, entity);
            }
        }

        var invalidEntities = new Dictionary<EntityUid, EntityUid>();

        foreach (var entity in validEntities)
        {

            var queryHelmets = EntityQueryEnumerator<IdentityBlockerComponent>();
            while (queryHelmets.MoveNext(out var helmet, out _))
            {
                var hands = CompOrNull<HandsComponent>(entity.Value);
                if (!_inventory.GetHandOrInventoryEntities(entity.Value, SlotFlags.HEAD).Contains(helmet))
                    continue;
                if (_inventory.GetHandOrInventoryEntities(entity.Value, SlotFlags.HEAD).Contains(helmet) &&
                    hands != null &&
                    _hands.IsHolding(entity.Value, helmet, out _, hands))
                    continue;

                validEntities.Remove(entity.Key);
                invalidEntities.TryAdd(entity.Key, entity.Value);
                break;
            }

            if (invalidEntities.ContainsKey(entity.Key))
                continue;
            _inventory.TryUnequip(entity.Value, "mask");
            _inventory.TryEquip(entity.Value, entity.Key, "mask");


        }
    }
}
