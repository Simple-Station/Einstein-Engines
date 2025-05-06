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
using Robust.Server.GameObjects;
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
    [Dependency] private readonly PhysicsSystem _phys = default!;
    [Dependency] private readonly TransformSystem _trans = default!;
    [Dependency] private readonly RayCastSystem _raycast = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;

    [Dependency] private readonly ILogManager _logs = default!;
    // im so sorry , SPCR 2025
    ConcurrentQueue<Tuple<StartCollideEvent, StartCollideEvent>> eventQueue = new();
    private EntityQuery<PhysicsComponent> physQuery;
    private EntityQuery<FixturesComponent> fixtureQuery;
    private EntityQuery<TransformComponent> transformQuery;
    private EntityQuery<MetaDataComponent> metaDataQuery;

    private ProcessRaycastsJob raycastsJob;

    public required ISawmill sawLogs;

    private record struct ProcessRaycastsJob(RayCastSystem RaySys, EntityQuery<PhysicsComponent>  Phys, EntityQuery<FixturesComponent> Fixt) : IParallelRobustJob
    {
        public int BatchSize => 100;
        public List<RaycastQuery> queries;
        public ConcurrentQueue<Tuple<StartCollideEvent, StartCollideEvent>> eventQueue;
        public void Execute(int index)
        {
            RaycastQuery data = queries[index];
            RayResult result = new();
            QueryFilter queryFilter = new QueryFilter()
            {
                LayerBits = data.CollisionBitMask,
                MaskBits = data.CollisionBitMask,
            };
            RaySys.CastRay(data.owner, ref result, data.start, data.translation, queryFilter, true);
            var bulletPhysics = Phys.GetComponent(data.owner);
            var bulletFixtures = Fixt.GetComponent(data.owner);
            var bulletString = bulletFixtures.Fixtures.Keys.First();
            foreach (var hit in result.Results)
            {
                if (hit.Entity == data.owner)
                    continue;
                var targetPhysics = Phys.GetComponent(hit.Entity);
                var targetFixtures = Fixt.GetComponent(hit.Entity);
                var targetString = targetFixtures.Fixtures.Keys.First();
                // i hate how verbose this is. - SPCR 2025
                var bulletEvent = new StartCollideEvent(
                    data.owner,
                    hit.Entity,
                    bulletString,
                    targetString,
                    bulletFixtures.Fixtures[bulletString],
                    targetFixtures.Fixtures[targetString],
                    bulletPhysics,
                    targetPhysics,
                    hit.Point);
                var targetEvent = new StartCollideEvent(
                    hit.Entity,
                    data.owner,
                    targetString,
                    bulletString,
                    targetFixtures.Fixtures[targetString],
                    bulletFixtures.Fixtures[bulletString],
                    targetPhysics,
                    bulletPhysics,
                    hit.Point);
                eventQueue.Enqueue(new Tuple<StartCollideEvent, StartCollideEvent>(bulletEvent, targetEvent));
            }


        }
    }

    internal readonly record struct RaycastQuery(
        EntityUid owner,
        Vector2 start,
        Vector2 translation,
        int CollisionBitMask);

    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectilePhasePreventComponent, MapInitEvent>(OnInit);
        raycastsJob = new ProcessRaycastsJob(_raycast, physQuery, fixtureQuery);
        fixtureQuery = GetEntityQuery<FixturesComponent>();
        physQuery = GetEntityQuery<PhysicsComponent>();
        sawLogs = _logs.GetSawmill("Phase-Prevention");
    }

    private void OnInit(EntityUid uid, ProjectilePhasePreventComponent comp, ref MapInitEvent args)
    {
        comp.start = _trans.GetWorldPosition(uid);
    }


    public override void Update(float frametime)
    {
        var enumerator =
            EntityQueryEnumerator<ProjectilePhasePreventComponent, PhysicsComponent, FixturesComponent,
                ProjectileComponent>();
        var rayCount = 0;
        raycastsJob.queries = new();
        ConcurrentQueue<Tuple<StartCollideEvent, StartCollideEvent>> eventQueue = new();
        raycastsJob.eventQueue = eventQueue;

        while (enumerator.MoveNext(out var owner, out var phaseComp, out var physComp, out var fixtComp,
                   out var projComp))
        {
            var map = _trans.GetMapId(owner);
            if (map == MapId.Nullspace)
            {
                continue;
            }
            var start = phaseComp.start;
            var end = _trans.GetWorldPosition()
            if (start == end)
                continue;
            raycastsJob.queries.Add(new RaycastQuery(owner, start, end - start, phaseComp.relevantBitmasks));
            rayCount++;
        }
        if(rayCount > 150)
            sawLogs.Info($"Processing {rayCount} raycasts.");
        _parallel.ProcessNow(raycastsJob, rayCount);

        while(eventQueue.TryDequeue(out var eventData))
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
        }


    }
}
