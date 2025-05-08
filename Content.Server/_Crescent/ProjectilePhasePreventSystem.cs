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
///  This was initially using RobustParallel Manager, but the implementation is horrendous for this kind of task (threadPooling (fake c# threads) instead of high-performance single CPU threads)
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
    private List<RaycastBucket> processingBuckets = new();
    public required ISawmill sawLogs;
    /// <summary>
    ///  how many rays will be put per thread
    ///
    /// </summary>
    internal const int raysPerThread = 150;

    private class RaycastBucket()
    {
        public HashSet<Entity<ProjectilePhasePreventComponent>> items = new();
        public List<Tuple<StartCollideEvent, StartCollideEvent>> output = new();

    };

    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectilePhasePreventComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<ProjectilePhasePreventComponent, ComponentShutdown>(OnRemove);
        processingBuckets.Add(new RaycastBucket());
        fixtureQuery = GetEntityQuery<FixturesComponent>();
        physQuery = GetEntityQuery<PhysicsComponent>();
        sawLogs = _logs.GetSawmill("Phase-Prevention");
    }

    private void OnInit(EntityUid uid, ProjectilePhasePreventComponent comp, ref ComponentStartup args)
    {
        comp.start = _trans.GetWorldPosition(uid);
        comp.mapId = _trans.GetMapId(uid);
        //comp.owner = uid;
        comp.ignoredEntities.Add(uid);
        foreach(var (key , fixture) in Comp<FixturesComponent>(uid).Fixtures)
        {
            comp.relevantBitmasks |= fixture.CollisionLayer;
        };
        if (processingBuckets.Last().items.Count >= raysPerThread)
        {
            processingBuckets.Add(new RaycastBucket());
        }
        processingBuckets.Last().items.Add((uid, comp));
        comp.containedAt = processingBuckets.Count - 1;

    }

    private void OnRemove(EntityUid uid, ProjectilePhasePreventComponent comp, ref ComponentShutdown args)
    {
        processingBuckets[comp.containedAt].items.Remove((uid, comp));
        if (processingBuckets[comp.containedAt].items.Count == 0 && comp.containedAt != 0)
        {
            processingBuckets.RemoveAt(comp.containedAt);
        }
        comp.ignoredEntities.Clear();
    }


    public override void Update(float frametime)
    {
        Parallel.ForEach(processingBuckets, args =>
        {
            QueryFilter queryFilter = new QueryFilter();
            args.output.Clear();
            Vector2 worldPos = Vector2.Zero;
            foreach(var (owner,  phase) in args.items)
            {
                // will be removed through events. Just skip for now
                if (TerminatingOrDeleted(owner))
                    continue;
                queryFilter.LayerBits = phase.relevantBitmasks;
                queryFilter.MaskBits = phase.relevantBitmasks;
                worldPos = _trans.GetWorldPosition(owner);
                RayResult result = _raycast.CastRay(phase.mapId, phase.start, worldPos - phase.start , queryFilter);
                phase.start = worldPos;
                var bulletPhysics = physQuery.GetComponent(owner);
                var bulletFixtures = fixtureQuery.GetComponent(owner);
                var bulletString = bulletFixtures.Fixtures.Keys.First();
                foreach (var hit in result.Results)
                {
                    // whilst the raycast supports a filter function . i do not want to package stuff in bulk
                    if(phase.ignoredEntities.Contains(hit.Entity))
                        continue;
                    var targetPhysics = physQuery.GetComponent(hit.Entity);
                    var targetFixtures = fixtureQuery.GetComponent(hit.Entity);
                    var targetString = targetFixtures.Fixtures.Keys.First();
                    // i hate how verbose this is. - SPCR 2025
                    var bulletEvent = new StartCollideEvent(
                        owner,
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
                        owner,
                        targetString,
                        bulletString,
                        targetFixtures.Fixtures[targetString],
                        bulletFixtures.Fixtures[bulletString],
                        targetPhysics,
                        bulletPhysics,
                        hit.Point);
                    args.output.Add(new Tuple<StartCollideEvent, StartCollideEvent>(bulletEvent, targetEvent));
                }

            }
        });

        foreach (var bucket in processingBuckets)
        {
            foreach(var eventData in bucket.output)
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
}
