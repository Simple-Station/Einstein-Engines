using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Crescent;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Threading;


public sealed class ProjectilePhasePreventerSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _phys = default!;
    [Dependency] private readonly SharedTransformSystem _trans = default!;

    [Dependency] private readonly ILogManager _logs = default!;
    // im so sorry , SPCR 2025
    ConcurrentQueue<Tuple<StartCollideEvent, StartCollideEvent>> eventQueue = new();
    private EntityQuery<PhysicsComponent> physQuery;
    private EntityQuery<FixturesComponent> fixtureQuery;
    private EntityQuery<TransformComponent> transformQuery;
    private EntityQuery<MetaDataComponent> metaDataQuery;

    public required ISawmill sawLogs;

    private record struct ProcessRaycastsJob : IParallelRobustJob
    {
        public int BatchSize => 100;
        public List<RaycastQuery> queries;
        public ConcurrentQueue<Tuple<StartCollideEvent, StartCollideEvent>> eventQueue;
        public void Execute(int index)
        {
            RaycastQuery data = queries[index];
            if (!transformQuery.GetComponent(data.owner, out var transform))
                return;


        }
    }

    internal readonly record struct RaycastQuery
    {
        public EntityUid owner { get; }
        public Vector2 start{ get; }
        public Vector2 end{ get; }
    }

    internal sealed class RaycastBucket
    {
        public EntityUid owner;
        public EntityUid? shooter;
        public Vector2 start;
        public Vector2 end;
        public string fixtureKey;
        public Robust.Shared.Physics.Dynamics.Fixture fixture;
        public PhysicsComponent physComp;
        public ProjectilePhasePreventComponent phaseComp;
        public EntityUid? projectileGrid;
        public MapId map;

        public RaycastBucket(string key, Robust.Shared.Physics.Dynamics.Fixture fix, PhysicsComponent component, ProjectilePhasePreventComponent phase)
        {
            fixtureKey = key;
            fixture = fix;
            physComp = component;
            phaseComp = phase;
        }
    }

    internal sealed class RaycastThreadBucketHolder
    {
        public int rayCount = 0;
        public List<RaycastBucket> buckets = new();
    }

    internal const int rayLimit = 150;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectilePhasePreventComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<ProjectilePhasePreventComponent, MoveEvent>(OnMove);
        sawLogs = _logs.GetSawmill("Phase-Prevention");
    }

    private void OnInit(EntityUid uid, ProjectilePhasePreventComponent comp, ref MapInitEvent args)
    {
        comp.start = _trans.GetWorldPosition(uid);
    }
    private void OnMove(EntityUid uid, ProjectilePhasePreventComponent comp, ref MoveEvent args)
    {
        if (args.NewPosition != EntityCoordinates.Invalid)
            comp.end = _trans.ToMapCoordinates(args.NewPosition).Position;
        else
            comp.end = Vector2.Zero;
    }
    private void ProcessBucket(RaycastThreadBucketHolder bucket, ParallelLoopState state, long indexer)
    {
        foreach (var raycast in bucket.buckets)
        {
            var owner = raycast.owner;
            var start = raycast.start;
            var end = raycast.end;
            var angle = (end - start).Normalized();
            var map = raycast.map;
            var physComp = raycast.physComp;
            CollisionRay ray = new CollisionRay(start, angle, (int)(CollisionGroup.BulletImpassable | CollisionGroup.Impassable));

            foreach (var obj in _phys.IntersectRay(map, ray, (end - start).Length(), owner, false))
            {
                if (obj.HitEntity == raycast.shooter)
                    continue;
                if (TerminatingOrDeleted(obj.HitEntity))
                    continue;
                if (TerminatingOrDeleted(owner))
                    break;
                if (!physQuery.TryGetComponent(obj.HitEntity, out var targPhysComp))
                    continue;
                if (!fixtureQuery.TryGetComponent(obj.HitEntity, out var targFixtComp))
                    continue;
                var targetGrid = _trans.GetGrid(obj.HitEntity);
                if (targetGrid is not null && targetGrid == raycast.projectileGrid)
                    continue;
                var ev = new StartCollideEvent(owner, obj.HitEntity, raycast.fixtureKey,
                    targFixtComp.Fixtures.Keys.First(), raycast.fixture, targFixtComp.Fixtures.Values.First(), physComp,
                    targPhysComp, obj.HitPos);
                var revEv = new StartCollideEvent(obj.HitEntity, owner, ev.OtherFixtureId, ev.OurFixtureId,
                    ev.OtherFixture, ev.OurFixture, targPhysComp, physComp, obj.HitPos);
                eventQueue.Enqueue(new Tuple<StartCollideEvent, StartCollideEvent>(ev, revEv));
            }
        }
    }

    public override void Update(float frametime)
    {
        var enumerator =
            EntityQueryEnumerator<ProjectilePhasePreventComponent, PhysicsComponent, FixturesComponent,
                ProjectileComponent>();
        fixtureQuery = GetEntityQuery<FixturesComponent>();
        physQuery = GetEntityQuery<PhysicsComponent>();
        var threadBuckets = new List<RaycastThreadBucketHolder>();
        var fillingBucket = new RaycastThreadBucketHolder();
        var rayCount = 0;

        while (enumerator.MoveNext(out var owner, out var phaseComp, out var physComp, out var fixtComp,
                   out var projComp))
        {
            var map = _trans.GetMapId(owner);
            if (map == MapId.Nullspace)
            {
                continue;
            }


            if (fillingBucket.rayCount > rayLimit)
            {
                threadBuckets.Add(fillingBucket);
                fillingBucket = new RaycastThreadBucketHolder();
            }

            var start = phaseComp.start;
            var end = phaseComp.end;
            if (start == end)
                continue;
            //Logger.Error($"Processing path: starting at {start.X}, {start.Y} and ending at {end.X}, {end.Y}");
            var bucket = new RaycastBucket(fixtComp.Fixtures.Keys.First(), fixtComp.Fixtures.Values.First(), physComp, phaseComp)
            { end = end, start = start };
            bucket.owner = owner;
            bucket.shooter = projComp.Shooter;
            bucket.start = phaseComp.start;
            bucket.end = phaseComp.end;
            bucket.map = map;
            if(projComp.Shooter is not null)
                bucket.projectileGrid = Transform(projComp.Shooter.Value).GridUid;
            //Logger.Error($"Surface area is {surfaceArea}");
            fillingBucket.rayCount++;
            rayCount++;
            fillingBucket.buckets.Add(bucket);
        }

        if (!threadBuckets.Contains(fillingBucket))
        {
            //Logger.Error($"Added bucket with {fillingBucket.buckets.Count} rays and {fillingBucket.medianCastArea} surface");
            threadBuckets.Add(fillingBucket);
        }

        eventQueue = new();
        //Logger.Error($"Processing {threadBuckets.Count} buckets");
        if(rayCount > 150)
            sawLogs.Info($"Processing {rayCount} raycasts.");
        Parallel.ForEach(threadBuckets, ProcessBucket);

        //if(eventQueue.Count != 0)
        //    Logger.Error($"Processing {eventQueue.Count} events!. Actual bullet count {eventQueue.Count/2}");

        // whilst i'd prefer this to be a HashSet, there has to be order in processing these.
        List<Tuple<StartCollideEvent, StartCollideEvent>> processingQueue = eventQueue.ToList();
        foreach(var eventData in processingQueue)
        {
            if (TerminatingOrDeleted(eventData.Item1.OurEntity) || TerminatingOrDeleted(eventData.Item2.OurEntity))
                continue;
            var fEv = eventData.Item1;
            var sEv = eventData.Item2;
            try
            {
                RaiseLocalEvent(eventData.Item1.OurEntity, ref fEv, true);
                RaiseLocalEvent(eventData.Item2.OurEntity, ref sEv, true);
            }
            catch (Exception e)
            {
                sawLogs.Error(e.Message);
            }


            //Logger.Error($"Tried to collide with {MetaData(eventData.collideEvent.OtherEntity).EntityName}");
        }


    }
}
