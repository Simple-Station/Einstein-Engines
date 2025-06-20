using System.Numerics;
using Content.Server.Parallax;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Parallax;
using Content.Shared._Crescent.SpaceBiomes;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Server.Station.Components;
using Content.Shared._Crescent.Vessel;

namespace Content.Server._Crescent.SpaceBiomes;

public sealed class SpaceBiomeSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly TransformSystem _formSys = default!;
    [Dependency] private readonly ParallaxSystem _parallaxSys = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private Dictionary<Vector2, HashSet<EntityUid>> _chunks = new();
    private float _updTimer;

    //if false, biomes will only be selected by chunks and not by their actual distance to the player
    private const bool PreciseRange = true;
    private const int ChunkSize = 1000; //in meters
    private const float UpdateInterval = 5; //in seconds

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpaceBiomeSourceComponent, ComponentInit>(OnSourceInit);
        SubscribeLocalEvent<SpaceBiomeSourceComponent, ComponentShutdown>(OnSourceShutdown);
        SubscribeLocalEvent<SpaceBiomeTrackerComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updTimer += frameTime;
        if (_updTimer < UpdateInterval)
            return;
        _updTimer = 0;

        foreach (ICommonSession session in _playerMan.Sessions)
        {
            if (session.AttachedEntity == null)
                continue;

            Vector2 playerPos = _formSys.GetWorldPosition(Transform(session.AttachedEntity.Value));
            SpaceBiomeTrackerComponent tracker = EnsureComp<SpaceBiomeTrackerComponent>(session.AttachedEntity.Value);

            HashSet<EntityUid> sourceUids = new();
            if (_chunks.TryGetValue((playerPos / ChunkSize).Floored() * ChunkSize, out var uids))
                sourceUids = uids;

            SpaceBiomeSourceComponent? newSource = null;
            foreach (EntityUid sourceUid in sourceUids)
            {
                SpaceBiomeSourceComponent source = Comp<SpaceBiomeSourceComponent>(sourceUid);

                if (PreciseRange && (_formSys.GetWorldPosition(sourceUid) - playerPos).Length() > source.SwapDistance)
                    continue;

                if (newSource == null ||
                    source.Priority > newSource.Priority ||
                    source.Priority == newSource.Priority && source == tracker.Source)
                {
                    newSource = source;
                }
            }

            if (newSource == tracker.Source)
                continue;

            tracker.Source = newSource;
            tracker.Biome = newSource?.Biome ?? "default";
            Dirty(session.AttachedEntity.Value, tracker);
            SwapBiome(session, session.AttachedEntity.Value, newSource);
        }
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        _chunks.Clear();
    }

    private void OnSourceInit(Entity<SpaceBiomeSourceComponent> uid, ref ComponentInit args)
    {
        AddBiome(uid, uid.Comp);
    }

    private void OnSourceShutdown(Entity<SpaceBiomeSourceComponent> uid, ref ComponentShutdown args)
    {
        RemoveBiome(uid, uid.Comp);
    }

    private void OnParentChanged(EntityUid uid, SpaceBiomeTrackerComponent component, EntParentChangedMessage args)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var parentStation = _stationSystem.GetOwningStation(uid);

        if (parentStation == null)
            return;

        if (!component.BoringStations.Contains((EntityUid) parentStation))
        {
            component.BoringStations.Add((EntityUid) parentStation);

            if (!TryComp<VesselDesignationComponent>(parentStation, out var desig) || !TryComp<StationNameSetupComponent>(parentStation, out var setup))
                return;

            var description = ""; //fallback if shuttle/station has no description

            if (TryComp<VesselDescriptionComponent>(parentStation, out var desc)) //if this succeeds, we have a description! if it fails,
                description = desc.Description;                                   //the component is missing and we just keep ""

            var name = setup.StationNameTemplate.Replace("{1}", "").Trim();
            // This is testing if we just initialized because we don't want to send the message on spawn
            if (_timing.CurTick.Value - MetaData(uid).CreationTick.Value > 500)
            {
                NewVesselEnteredMessage message = new NewVesselEnteredMessage(name, Loc.GetString(desig.Designation), description);
                RaiseNetworkEvent(message, actor.PlayerSession);
            }
        }
    }

    public void AddBiome(EntityUid uid, SpaceBiomeSourceComponent source)
    {
        foreach (Vector2 chunkPos in GetCoveredChunks(_formSys.GetWorldPosition(uid), source.SwapDistance))
        {
            if (!_chunks.ContainsKey(chunkPos))
                _chunks[chunkPos] = new();
            _chunks[chunkPos].Add(uid);
        }
    }

    //works assuming that biome source position and range haven't changed
    public void RemoveBiome(EntityUid uid, SpaceBiomeSourceComponent source)
    {
        foreach (Vector2 chunkPos in GetCoveredChunks(_formSys.GetWorldPosition(uid), source.SwapDistance))
        {
            if (_chunks.ContainsKey(chunkPos))
            {
                if (_chunks[chunkPos].Count == 1)
                {
                    _chunks.Remove(chunkPos);
                    continue;
                }
                _chunks[chunkPos].Remove(uid);
            }
        }
    }

    private void SwapBiome(ICommonSession session, EntityUid uid, SpaceBiomeSourceComponent? source)
    {
        EntityUid? mapUid = _formSys.GetMap(session.AttachedEntity ?? EntityUid.Invalid);
        if (mapUid == null)
            return;

        SpaceBiomePrototype biome = _protMan.Index<SpaceBiomePrototype>(source?.Biome ?? "default");
        _parallaxSys.SwapParallax(uid, EnsureComp<ParallaxComponent>(uid), biome.Parallax, biome.SwapDuration);

        SpaceBiomeSwapMessage msg = new() { Biome = source?.Biome ?? "default" };
        RaiseNetworkEvent(msg, session);
    }

    private List<Vector2> GetCoveredChunks(Vector2 pos, int radius)
    {
        List<Vector2> result = new();
        Vector2 posFloor = (pos / ChunkSize).Floored() * ChunkSize;

        int chunks = (radius + ChunkSize - 1) / ChunkSize; //ceil of int division
        for (int y = -chunks; y <= chunks; y++)
        {
            for (int x = -chunks; x <= chunks; x++)
            {
                Vector2 chunkPos = new Vector2(x * ChunkSize, y * ChunkSize) + posFloor;
                if (CrescentHelpers.RectCircleIntersect(
                    new Box2(chunkPos, chunkPos + new Vector2(ChunkSize)),
                    pos,
                    radius))
                {
                    result.Add(chunkPos);
                }
            }
        }

        return result;
    }

    public void RegenerateChunks()
    {
        _chunks.Clear();
        var query = EntityQueryEnumerator<SpaceBiomeSourceComponent>();

        while (query.MoveNext(out var uid, out var source))
        {
            AddBiome(uid, source);
        }
    }
}
