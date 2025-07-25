using System.Linq;
using Content.Server.Paint;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.CCVar;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Clothing.Systems;

public sealed class LoadoutSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly SharedLoadoutSystem _loadout = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly PaintSystem _paint = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ILogManager _log = default!;

    private ISawmill _sawmill = default!;


    public override void Initialize()
    {
        _sawmill = _log.GetSawmill("loadouts");

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<LoadProfileExtensionsEvent>(OnProfileLoad);
    }


    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId == null || Deleted(ev.Mob) || !Exists(ev.Mob)
            || !HasComp<MetaDataComponent>(ev.Mob) // TODO: FIND THE STUPID RACE CONDITION THAT IS MAKING ME CHECK FOR THIS.
            || !_protoMan.TryIndex<JobPrototype>(ev.JobId, out var job)
            || !_configurationManager.GetCVar(CCVars.GameLoadoutsEnabled))
            return;

        ApplyCharacterLoadout(
            ev.Mob,
            ev.JobId,
            ev.Profile,
            _playTimeTracking.GetTrackerTimes(ev.Player),
            ev.Player.ContentData()?.Whitelisted ?? false,
            jobProto: job);

        RaiseLocalEvent(ev.Mob, new PlayerLoadoutAppliedEvent(ev.Mob, ev.Player, ev.JobId, ev.LateJoin, ev.Silent, ev.JoinOrder, ev.Station, ev.Profile), broadcast: true);
    }

    private void OnProfileLoad(LoadProfileExtensionsEvent ev)
    {
        if (ev.JobId == null || Deleted(ev.Mob) || !Exists(ev.Mob)
            || !HasComp<MetaDataComponent>(ev.Mob) // TODO: FIND THE STUPID RACE CONDITION THAT IS MAKING ME CHECK FOR THIS.
            || !_protoMan.TryIndex<JobPrototype>(ev.JobId, out var job)
            || !_configurationManager.GetCVar(CCVars.GameLoadoutsEnabled))
            return;

        ApplyCharacterLoadout(
            ev.Mob,
            ev.JobId,
            ev.Profile,
            _playTimeTracking.GetTrackerTimes(ev.Player),
            ev.Player.ContentData()?.Whitelisted ?? false,
            jobProto: job);
    }


    /// Equips every loadout, then puts whatever extras it can in inventories
    public void ApplyCharacterLoadout(
        EntityUid uid,
        ProtoId<JobPrototype> job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        bool deleteFailed = false,
        JobPrototype? jobProto = null)
    {
        // Spawn the loadout, get a list of items that failed to equip
        var (failedLoadouts, allLoadouts) =
            _loadout.ApplyCharacterLoadout(uid, job, profile, playTimes, whitelisted, out var heirlooms);

        // Try to find back-mounted storage apparatus
        if (_inventory.TryGetSlotEntity(uid, "back", out var item) &&
            EntityManager.TryGetComponent<StorageComponent>(item, out var inventory))
            // Try inserting the entity into the storage, if it can't, it leaves the loadout item on the ground
            foreach (var loadout in failedLoadouts)
                if ((!EntityManager.TryGetComponent<ItemComponent>(loadout, out var itemComp)
                        || !_storage.CanInsert(item.Value, loadout, out _, inventory, itemComp)
                        || !_storage.Insert(item.Value, loadout, out _, playSound: false))
                    && deleteFailed)
                    EntityManager.QueueDeleteEntity(loadout);

        foreach (var loadout in allLoadouts)
        {
            if (loadout.Item1 == EntityUid.Invalid
                || !HasComp<MetaDataComponent>(loadout.Item1)
                || Deleted(loadout.Item1))
            {
                _sawmill.Warning($"Loadout {loadout.Item2.LoadoutName} failed to load properly, deleting.");
                EntityManager.QueueDeleteEntity(loadout.Item1);

                continue;
            }

            var loadoutProto = _protoMan.Index<LoadoutPrototype>(loadout.Item2.LoadoutName);
            if (loadoutProto.CustomName && loadout.Item2.CustomName != null)
                _meta.SetEntityName(loadout.Item1, loadout.Item2.CustomName);
            if (loadoutProto.CustomDescription && loadout.Item2.CustomDescription != null)
                _meta.SetEntityDescription(loadout.Item1, loadout.Item2.CustomDescription);
            if (loadoutProto.CustomColorTint && !string.IsNullOrEmpty(loadout.Item2.CustomColorTint))
                _paint.Paint(null, null, loadout.Item1, Color.FromHex(loadout.Item2.CustomColorTint));

            foreach (var component in loadoutProto.Components.Values)
            {
                if (HasComp(loadout.Item1, component.Component.GetType()))
                    continue;

                var comp = (Component) _serialization.CreateCopy(component.Component, notNullableOverride: true);
                comp.Owner = loadout.Item1;
                EntityManager.AddComponent(loadout.Item1, comp);
            }

            foreach (var function in loadoutProto.Functions)
                function.OnPlayerSpawn(uid, loadout.Item1, _componentFactory, EntityManager, _serialization);
        }

        // Pick the heirloom
        if (heirlooms.Any())
        {
            var heirloom = _random.Pick(heirlooms);
            EnsureComp<HeirloomHaverComponent>(uid, out var haver);
            EnsureComp<HeirloomComponent>(heirloom.Item1, out var comp);
            haver.Heirloom = heirloom.Item1;
            comp.HOwner = uid;
            Dirty(uid, haver);
            Dirty(heirloom.Item1, comp);
        }

        if (jobProto != null ||
            _protoMan.TryIndex(job, out jobProto))
            foreach (var special in jobProto.AfterLoadoutSpecial)
                special.AfterEquip(uid);
    }
}
