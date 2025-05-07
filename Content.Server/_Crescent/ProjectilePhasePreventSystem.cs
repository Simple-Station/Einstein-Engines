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
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using MathNet.Numerics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Robust.Server.GameObjects;
using Robust.Shared.Collections;
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
using Robust.Shared.Toolshed.Commands.Debug;

/// <summary>
///  Written by MLGTASTICa/SPCR 2025 for Hullrot EE
///  This was initially using RobustParallel Manager, but the implementation is horrendous for this kind of task (threadPooling instead of high-performance CPU threads)
/// </summary>
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

    public required ISawmill sawLogs;
    /// <summary>
    ///  how many rays will be put per thread
    ///  
    /// </summary>
    internal const int raysPerThread = 450;


    private record struct RaycastBucket(RaycastQuery[] Queries, int space);

    private record struct ProcessRaycastsJob(RayCastSystem RaySys, EntityQuery<PhysicsComponent>  Phys, EntityQuery<FixturesComponent> Fixt) : IParallelRobustJob
    {
        public int BatchSize => 100;
        public List<RaycastQuery> queries;
        public ConcurrentQueue<Tuple<StartCollideEvent, StartCollideEvent>> eventQueue;
        public void Execute(int index)
        {
            RaycastQuery data = queries[index];
            QueryFilter queryFilter = new QueryFilter()
            {
                LayerBits = data.CollisionBitMask,
                MaskBits = data.CollisionBitMask,
            };
            RayResult result = RaySys.CastRay(data.map, data.start, data.translation, queryFilter);
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
        MapId map,
        int CollisionBitMask);

    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectilePhasePreventComponent, MapInitEvent>(OnInit);
        fixtureQuery = GetEntityQuery<FixturesComponent>();
        physQuery = GetEntityQuery<PhysicsComponent>();
        sawLogs = _logs.GetSawmill("Phase-Prevention");
    }

    private void OnInit(EntityUid uid, ProjectilePhasePreventComponent comp, ref MapInitEvent args)
    {
        comp.start = _trans.GetWorldPosition(uid);
        foreach(var (key , fixture) in Comp<FixturesComponent>(uid).Fixtures)
        {
            comp.relevantBitmasks |= fixture.CollisionLayer;
        };
    }


    public override void Update(float frametime)
    {
        
        var enumerator =
            EntityQueryEnumerator<ProjectilePhasePreventComponent, PhysicsComponent, FixturesComponent,
                ProjectileComponent>();
        var rayCount = 0;
        var currentBucket = new RaycastBucket(new RaycastQuery[raysPerThread], raysPerThread);
        List<RaycastBucket> buckets = new List<RaycastBucket>();
        ConcurrentQueue<Tuple<StartCollideEvent, StartCollideEvent>> eventQueue = new();

        while (enumerator.MoveNext(out var owner, out var phaseComp, out var physComp, out var fixtComp,
                   out var projComp))
        {
            if (TerminatingOrDeleted(owner))
                continue;
            var map = _trans.GetMapId(owner);
            if (map == MapId.Nullspace)
            {
                continue;
            }
            var start = phaseComp.start;
            var end = _trans.GetWorldPosition(owner);
            if (start == end)
                continue;
            if(currentBucket.space == 0)
            {
                buckets.Add(currentBucket);
                currentBucket = new RaycastBucket(new RaycastQuery[raysPerThread], raysPerThread); 
            }
            currentBucket.Queries[currentBucket.space-- - 1] = new RaycastQuery(owner, start, end - start, map, phaseComp.relevantBitmasks);
            rayCount++;
        }
        Parallel.ForEach<RaycastBucket>(buckets, args =>
        {
            for(int i = raysPerThread - 1; i > args.space; i--)
            {
                RaycastQuery data = args.Queries[i];
                QueryFilter queryFilter = new QueryFilter()
                {
                    LayerBits = data.CollisionBitMask,
                    MaskBits = data.CollisionBitMask,
                };
                RayResult result = _raycast.CastRay(data.map, data.start, data.translation, queryFilter);
                var bulletPhysics = physQuery.GetComponent(data.owner);
                var bulletFixtures = fixtureQuery.GetComponent(data.owner);
                var bulletString = bulletFixtures.Fixtures.Keys.First();
                foreach (var hit in result.Results)
                {
                    if (hit.Entity == data.owner)
                        continue;
                    var targetPhysics = physQuery.GetComponent(hit.Entity);
                    var targetFixtures = fixtureQuery.GetComponent(hit.Entity);
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
        });


        if(rayCount > 150)
            sawLogs.Info($"Processing {rayCount} raycasts.");

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
