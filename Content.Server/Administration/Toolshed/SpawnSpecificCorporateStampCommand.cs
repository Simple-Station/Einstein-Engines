using Content.Server.GameTicking;
using Content.Shared._EE.Contractors.Systems;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Content.Shared._EE.Contractors.Components;
using Content.Shared._EE.Contractors.Prototypes;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Prototypes;


namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AdminCommand(AdminFlags.Debug)]
public sealed class SpawnSpecificCorporateStampCommand : ToolshedCommand
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    private GameTicker? _ticker;
    private SharedTransformSystem? _sharedTransformSystem;
    private InventorySystem? _inventory;
    private SharedStorageSystem? _storage;
    private CorporateStampSystem? _corporateStampSystem;

    [CommandImplementation]
    public void SpawnSpecificCorporateStamp(IInvocationContext ctx, [CommandArgument] ProtoId<EmployerPrototype> input, [CommandArgument] bool command = false)
    {
        _ticker ??= GetSys<GameTicker>();

        if (ExecutingEntity(ctx) is not { } ent)
        {
            if (ctx.Session is {} session)
                ctx.ReportError(new SessionHasNoEntityError(session));
            else
                ctx.ReportError(new NotForServerConsoleError());
        }
        else
        {
            if (!TryComp(ent, out ActorComponent? targetActor))
                return;

            var profile = _ticker.GetPlayerProfile(targetActor.PlayerSession);
            SpawnStampForPlayer(ent, input, command);
        }
    }

    public void SpawnStampForPlayer(EntityUid mob, ProtoId<EmployerPrototype> employer, bool command = false)
    {
        _sharedTransformSystem ??= GetSys<SharedTransformSystem>();
        _inventory ??= GetSys<InventorySystem>();
        _storage ??= GetSys<SharedStorageSystem>();
        _corporateStampSystem ??= GetSys<CorporateStampSystem>();

        if (Deleted(mob))
            return;

        if (!_prototypeManager.TryIndex("RubberStampCorporateLiaison", out EntityPrototype? entityPrototype))
            return;

        var stampEntity = _entityManager.SpawnEntity(entityPrototype.ID, _sharedTransformSystem.GetMapCoordinates(mob));
        var corporateStampComponent = _entityManager.GetComponent<CorporateStampComponent>(stampEntity);

        _corporateStampSystem.UpdateCorporateStamp(stampEntity, employer, command);

        // Try to find back-mounted storage apparatus
        if (_inventory.TryGetSlotEntity(mob, "back", out var item) &&
                EntityManager.TryGetComponent<StorageComponent>(item, out var inventory))
            // Try inserting the entity into the storage, if it can't, it leaves the loadout item on the ground
        {
            if (!EntityManager.TryGetComponent<ItemComponent>(stampEntity, out var itemComp)
                || !_storage.CanInsert(item.Value, stampEntity, out _, inventory, itemComp)
                || !_storage.Insert(item.Value, stampEntity, out _, playSound: false))
            {
                _adminLogManager.Add(
                    LogType.EntitySpawn,
                    LogImpact.Low,
                    $"Stamp for {employer} was spawned on the floor due to missing bag space");
            }
        }
    }
}
