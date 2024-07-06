using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Spawners.Components;
using Robust.Shared.Random;
using Robust.Server.Player;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Storage;
using Robust.Shared.Utility;
using System.Threading;
using Content.Server.Actions;
using Content.Server.Backmen.Blob.Components;
using Content.Server.Backmen.Blob.Rule;
using Content.Server.Backmen.GameTicking.Rules.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.RandomMetadata;
using Content.Shared.Backmen.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Backmen.SpecForces;

public sealed class SpecForcesSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    [ViewVariables] public List<SpecForcesHistory> CalledEvents { get; } = new();
    [ViewVariables] public TimeSpan LastUsedTime { get; private set; } = TimeSpan.Zero;
    private readonly ReaderWriterLockSlim _callLock = new();
    private TimeSpan DelayUsage => TimeSpan.FromMinutes(_configurationManager.GetCVar(CCVars.SpecForceDelay));
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("specforce");

        SubscribeLocalEvent<SpecForceComponent, MapInitEvent>(OnMapInit, after: new[] { typeof(RandomMetadataSystem) });
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);
        SubscribeLocalEvent<SpecForceComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SpecForceComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<BlobChangeLevelEvent>(OnBlobChange);
    }

    [ValidatePrototypeId<SpecForceTeamPrototype>]
    private const string Rxbzz = "RXBZZ";

    private void OnBlobChange(BlobChangeLevelEvent ev)
    {
<<<<<<< HEAD
        if(ev.Level == BlobStage.Critical)
            CallOps(Rxbzz, "ДСО");
||||||| parent of 54ef38c02d (Fix blob + specforce (#671))
        if (ev.Level != BlobStage.Critical)
            return;

        if (!CallOps(Rxbzz, "ДСО"))
        {
            Log.Error($"Failed to spawn {Rxbzz} SpecForce for the blob GameRule!");
        }
=======
        if (ev.Level != BlobStage.Critical)
            return;

        var blobConfig = CompOrNull<StationBlobConfigComponent>(ev.Station);
        var specForceTeam = blobConfig?.SpecForceTeam ?? Rxbzz;

        if (!_prototypes.TryIndex(specForceTeam, out var prototype) ||
            !CallOps(prototype.ID, "ДСО"))
        {
            Log.Error($"Failed to spawn {specForceTeam} SpecForce for the blob GameRule!");
        }
>>>>>>> 54ef38c02d (Fix blob + specforce (#671))
    }

    private void OnShutdown(EntityUid uid, SpecForceComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.BssKey);
    }

    private void OnStartup(EntityUid uid, SpecForceComponent component, ComponentStartup args)
    {
        if (component.ActionBssActionName != null)
            _actions.AddAction(uid, ref component.BssKey, component.ActionBssActionName);
    }

    private void OnMapInit(EntityUid uid, SpecForceComponent component, MapInitEvent args)
    {
        foreach (var entry in component.Components.Values)
        {
            var comp = (Component) _serialization.CreateCopy(entry.Component, notNullableOverride: true);
            comp.Owner = uid;
            EntityManager.AddComponent(uid, comp);
        }
    }

    public TimeSpan DelayTime
    {
        get
        {
            var ct = _gameTicker.RoundDuration();
            var lastUsedTime = LastUsedTime + DelayUsage;
            return ct > lastUsedTime ? TimeSpan.Zero : lastUsedTime - ct;
        }
    }

<<<<<<< HEAD
    public bool CallOps(ProtoId<SpecForceTeamPrototype> protoId, string source = "")
||||||| parent of 54ef38c02d (Fix blob + specforce (#671))
    /// <summary>
    /// Calls SpecForce team, creating new map with a shuttle, and spawning on it SpecForces.
    /// </summary>
    /// <param name="protoId"> SpecForceTeamPrototype ID.</param>
    /// <param name="source"> Source of the call.</param>
    /// <param name="forceCountExtra"> How many extra SpecForces will be forced to spawn.</param>
    /// <returns>Returns true if call was successful.</returns>
    /// <exception cref="ArgumentException"> If ProtoId of the team is invalid.</exception>
    public bool CallOps(ProtoId<SpecForceTeamPrototype> protoId, string source = "", int? forceCountExtra = null)
=======
    /// <summary>
    /// Calls SpecForce team, creating new map with a shuttle, and spawning on it SpecForces.
    /// </summary>
    /// <param name="protoId"> SpecForceTeamPrototype ID.</param>
    /// <param name="source"> Source of the call.</param>
    /// <param name="forceCountExtra"> How many extra SpecForces will be forced to spawn.</param>
    /// <returns>Returns true if call was successful.</returns>
    public bool CallOps(ProtoId<SpecForceTeamPrototype> protoId, string source = "", int? forceCountExtra = null)
>>>>>>> 54ef38c02d (Fix blob + specforce (#671))
    {
        if (!_callLock.TryEnterWriteLock(-1))
        {
            Log.Warning("SpecForces is busy!");
            return false;
        }
        try
        {
            if (_gameTicker.RunLevel != GameRunLevel.InRound)
            {
                Log.Warning("Can't call SpecForces while not in the round.");
                return false;
            }

            var currentTime = _gameTicker.RoundDuration();

#if !DEBUG
            if (LastUsedTime + DelayUsage > currentTime)
            {
                Log.Info("Tried to call SpecForce when it's on cooldown.");
                return false;
            }
#endif

            LastUsedTime = currentTime;

            if (!_prototypes.TryIndex(protoId, out var prototype))
            {
                throw new ArgumentException("Wrong SpecForceTeamPrototype ID!");
            }

            CalledEvents.Add(new SpecForcesHistory { Event = prototype.SpecForceName, RoundTime = currentTime, WhoCalled = source });

            var shuttle = SpawnShuttle(prototype.ShuttlePath);
            if (shuttle == null)
            {
                return false;
            }

            SpawnGhostRole(prototype, shuttle.Value);

            DispatchAnnouncement(prototype);

            return true;
        }
        finally
        {
            _callLock.ExitWriteLock();
        }
    }

    private EntityUid SpawnEntity(string? protoName, EntityCoordinates coordinates)
    {
        if (protoName == null)
        {
            return EntityUid.Invalid;
        }

        var uid = EntityManager.SpawnEntity(protoName, coordinates);

        if (!TryComp<GhostRoleMobSpawnerComponent>(uid, out var mobSpawnerComponent) ||
            mobSpawnerComponent.Prototype == null ||
            !_prototypes.TryIndex<EntityPrototype>(mobSpawnerComponent.Prototype, out var spawnObj))
        {
            return uid;
        }

        if (spawnObj.TryGetComponent<SpecForceComponent>(out var tplSpecForceComponent))
        {
            var comp = (Component) _serialization.CreateCopy(tplSpecForceComponent, notNullableOverride: true);
            comp.Owner = uid;
            EntityManager.AddComponent(uid, comp);
        }

        EnsureComp<SpecForceComponent>(uid);
        if (spawnObj.TryGetComponent<GhostRoleComponent>(out var tplGhostRoleComponent))
        {
            var comp = (Component) _serialization.CreateCopy(tplGhostRoleComponent, notNullableOverride: true);
            comp.Owner = uid;
            EntityManager.AddComponent(uid, comp);
        }

        return uid;
    }

    private void SpawnGhostRole(SpecForceTeamPrototype proto, EntityUid shuttle)
    {
        var spawns = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<SpawnPointComponent, MetaDataComponent, TransformComponent>();
        while (query.MoveNext(out _, out var meta, out var xform))
        {
            if (meta.EntityPrototype!.ID != proto.SpawnMarker)
                continue;

            if (xform.GridUid != shuttle)
                continue;

            spawns.Add(xform.Coordinates);
        }

        if (spawns.Count == 0)
        {
            _sawmill.Warning("Shuttle has no valid spawns for SpecForces! Making something up...");
            spawns.Add(Transform(shuttle).Coordinates);
        }

        SpawnGuaranteed(proto, spawns);
        SpawnSpecForces(proto, spawns, forceCountExtra);
    }

    private void SpawnGuaranteed(SpecForceTeamPrototype proto, List<EntityCoordinates> spawns)
    {
        // If specForceSpawn is empty, we can't continue
        if (proto.GuaranteedSpawn.Count == 0)
            return;

        // Spawn Guaranteed SpecForces from the prototype.
        var toSpawnGuaranteed = EntitySpawnCollection.GetSpawns(proto.GuaranteedSpawn, _random);
<<<<<<< HEAD
||||||| parent of 54ef38c02d (Fix blob + specforce (#671))

        var countGuaranteed = 0;
=======

>>>>>>> 54ef38c02d (Fix blob + specforce (#671))
        foreach (var mob in toSpawnGuaranteed)
        {
<<<<<<< HEAD
            var spawned = SpawnEntity(mob, _random.Pick(spawns));
            _sawmill.Info($"Successfully spawned {ToPrettyString(spawned)} SpecForce.");
||||||| parent of 54ef38c02d (Fix blob + specforce (#671))
            var spawned = SpawnEntity(mob, _random.Pick(spawns), proto);
            Log.Info($"Successfully spawned {ToPrettyString(spawned)} SpecForce.");
            countGuaranteed++;
=======
            var spawned = SpawnEntity(mob, _random.Pick(spawns), proto);
            Log.Info($"Successfully spawned {ToPrettyString(spawned)} Static SpecForce.");
>>>>>>> 54ef38c02d (Fix blob + specforce (#671))
        }
    }

<<<<<<< HEAD
||||||| parent of 54ef38c02d (Fix blob + specforce (#671))
        // Don't count entry's with have not 100% chance to spawn.
        // This way random will only help and won't hurt SpecForce team.
        /*
        foreach (var spawnEntry in proto.GuaranteedSpawn.Where(spawnEntry => spawnEntry.SpawnProbability < 1))
        {
            // We also need to check if this role was spawned by The Gods Of Random
            if (toSpawnGuaranteed.Contains(spawnEntry.PrototypeId!.Value))
                countGuaranteed--;
        }
        */

=======
    private void SpawnSpecForces(SpecForceTeamPrototype proto, List<EntityCoordinates> spawns, int? forceCountExtra)
    {
        // If specForceSpawn is empty, we can't continue
        if (proto.SpecForceSpawn.Count == 0)
            return;

>>>>>>> 54ef38c02d (Fix blob + specforce (#671))
        // Count how many other forces there should be.
<<<<<<< HEAD
        var countExtra = _playerManager.PlayerCount / proto.SpawnPerPlayers;
        countExtra = Math.Max(0, countExtra - proto.GuaranteedSpawn.Count); // Either zero or bigger than zero, no negatives
        countExtra = Math.Min(countExtra, proto.MaxRolesAmount - proto.GuaranteedSpawn.Count); // If bigger than MaxAmount, set to MaxAmount and extract already spawned roles

||||||| parent of 54ef38c02d (Fix blob + specforce (#671))
        var countExtra = GetOptIdCount(proto);
        // If bigger than MaxAmount, set to MaxAmount and extract already spawned roles
        countExtra = Math.Min(countExtra, proto.MaxRolesAmount);//Math.Min(countExtra - countGuaranteed, proto.MaxRolesAmount - countGuaranteed);

        // If CountExtra was forced to some number, check if this number is in range and extract already spawned roles.
        if (forceCountExtra is >= 0 and <= 15)
            countExtra = forceCountExtra.Value;// - countGuaranteed;

        // Either zero or bigger than zero, no negatives
        countExtra = Math.Max(0, countExtra);

        Log.Info($"Guaranteed spawned static {countGuaranteed} SpecForces, spawning opt-in {countExtra} more.");

=======
        var countExtra = GetOptIdCount(proto);
        // If bigger than MaxAmount, set to MaxAmount and extract already spawned roles
        countExtra = Math.Min(countExtra, proto.MaxRolesAmount);

        // If CountExtra was forced to some number, check if this number is in range and extract already spawned roles.
        if (forceCountExtra is >= 0 and <= 15)
            countExtra = forceCountExtra.Value;

        // Either zero or bigger than zero, no negatives
        countExtra = Math.Max(0, countExtra);

>>>>>>> 54ef38c02d (Fix blob + specforce (#671))
        // Spawn Guaranteed SpecForces from the prototype.
        // If all mobs from the list are spawned and we still have free slots, restart the cycle again.
        var toSpawnForces = EntitySpawnCollection.GetSpawns(proto.SpecForceSpawn, _random);
        while (countExtra > 0)
        {
            foreach (var mob in toSpawnForces.Where( _ => countExtra > 0))
            {
                countExtra--;
<<<<<<< HEAD
                var spawned = SpawnEntity(mob, _random.Pick(spawns));
                _sawmill.Info($"Successfully spawned {ToPrettyString(spawned)} SpecForce.");
||||||| parent of 54ef38c02d (Fix blob + specforce (#671))
                var spawned = SpawnEntity(mob, _random.Pick(spawns), proto);
                Log.Info($"Successfully spawned {ToPrettyString(spawned)} SpecForce.");
=======
                var spawned = SpawnEntity(mob, _random.Pick(spawns), proto);
                Log.Info($"Successfully spawned {ToPrettyString(spawned)} Opt-in SpecForce.");
>>>>>>> 54ef38c02d (Fix blob + specforce (#671))
            }
        }
    }

    /// <summary>
    /// Spawns shuttle for SpecForces on a new map.
    /// </summary>
    /// <param name="shuttlePath"></param>
    /// <returns>Grid's entity of the shuttle.</returns>
    private EntityUid? SpawnShuttle(string shuttlePath)
    {
        var shuttleMap = _mapManager.CreateMap();
        var options = new MapLoadOptions()
        {
            LoadMap = true
        };

        if (!_map.TryLoad(shuttleMap, shuttlePath, out var grids, options))
        {
            return null;
        }

        var mapGrid = grids.FirstOrNull();

        return mapGrid ?? null;
    }

    private void DispatchAnnouncement(SpecForceTeamPrototype proto)
    {
        var stations = _stationSystem.GetStations();
        var playTts = false;

        if (stations.Count == 0)
            return;

        if (proto.AnnouncementText == null || proto.AnnouncementTitle == null)
            return;

        if (proto.AnnouncementSoundPath == default!)
            playTts = true;

        foreach (var station in stations)
        {
            _chatSystem.DispatchStationAnnouncement(station,
                Loc.GetString(proto.AnnouncementText),
                Loc.GetString(proto.AnnouncementTitle),
                playTts, proto.AnnouncementSoundPath);
        }
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        foreach (var calledEvent in CalledEvents)
        {
            ev.AddLine(Loc.GetString("spec-forces-system-round-end",
                ("specforce", Loc.GetString(calledEvent.Event)),
                ("time", calledEvent.RoundTime.ToString(@"hh\:mm\:ss")),
                ("who", calledEvent.WhoCalled)));
        }
    }

    private void OnCleanup(RoundRestartCleanupEvent ev)
    {
        CalledEvents.Clear();
        LastUsedTime = TimeSpan.Zero;

        if (_callLock.IsWriteLockHeld)
        {
            _callLock.ExitWriteLock();
        }
    }
}
