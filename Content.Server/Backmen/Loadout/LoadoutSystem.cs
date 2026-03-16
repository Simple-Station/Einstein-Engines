using Content.Corvax.Interfaces.Server;
using Content.Corvax.Interfaces.Shared;
using Content.Server.GameTicking;
using Content.Server.Hands.Systems;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Clothing.Components;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Server.Backmen.Loadout;

public sealed class LoadoutSystem : EntitySystem
{
    private const string BackpackSlotId = "back";

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly StorageSystem _storageSystem = default!;
    [Dependency] private readonly ISharedSponsorsManager _sponsorsManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        return;
        if (!_sponsorsManager.TryGetServerPrototypes(ev.Player.UserId, out var prototypes))
            return;

        foreach (var loadoutId in prototypes)
        {
            if (!_prototypeManager.TryIndex<LoadoutPrototype>(loadoutId, out var loadout))
                continue;

            if (loadout.SponsorOnly && !prototypes.Contains(loadoutId))
                continue;

            var xform = Transform(ev.Mob);
            var coordinates = xform.Coordinates;

            foreach (var (slot, protoId) in loadout.Equipment)
            {
                var entity = Spawn(protoId, coordinates);

                if (!_inventorySystem.TryEquip(ev.Mob, entity, slot, true, force: true))
                {
                    if (_inventorySystem.TryGetSlotEntity(ev.Mob, BackpackSlotId, out var backEntity) &&
                        _storageSystem.CanInsert(backEntity.Value, entity, out _))
                    {
                        _storageSystem.Insert(backEntity.Value, entity, out _, playSound: false);
                    }
                    else
                    {
                        QueueDel(entity);
                    }
                }
            }

            foreach (var protoId in loadout.Inhand)
            {
                var entity = Spawn(protoId, coordinates);

                if (!_handsSystem.TryPickup(ev.Mob, entity))
                {
                    if (_inventorySystem.TryGetSlotEntity(ev.Mob, BackpackSlotId, out var backEntity) &&
                        _storageSystem.CanInsert(backEntity.Value, entity, out _))
                    {
                        _storageSystem.Insert(backEntity.Value, entity, out _, playSound: false);
                    }
                    else
                    {
                        QueueDel(entity);
                    }
                }
            }
        }
    }
}
