using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Shared._Crescent;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Threading;

/// <summary>
///  Written by MLGTASTICa/SPCR 2025 for Hullrot EE
///  This was initially using RobustParallel Manager, but the implementation is horrendous for this kind of task (threadPooling (fake c# threads) instead of high-performance single CPU threads)
///  This system is expected to be ran on objects that DO not have any HARD-FIXTURES. As in all-collision events are handled only by this and not by physics due to actual body collision.
/// </summary>
public class ProjectilePhasePreventerSystem : EntitySystem
{
    [Dependency] private readonly PhysicsSystem _phys = default!;
    [Dependency] private readonly TransformSystem _trans = default!;
    [Dependency] private readonly RayCastSystem _raycast = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly ILogManager _logs = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MapSystem _map = default!;
    private EntityQuery<PhysicsComponent> physQuery;
    private EntityQuery<FixturesComponent> fixtureQuery;
    private List<RaycastBucket> processingBuckets = new();
    public required ISawmill sawLogs;
    /// <summary>
    ///  how many rays will be put per thread
    ///
    /// </summary>
    internal const int raysPerThread = 150;

    public class RaycastBucket()
    {
        public HashSet<Entity<ProjectilePhasePreventComponent, ProjectileComponent>> items = new();
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
        if (!TryComp<ProjectileComponent>(uid, out var projectile))
        {
            sawLogs.Error($"Tried to initialize ProjectilePhasePreventComponent on entity without projectileComponent. Prototype : {MetaData(uid).EntityPrototype?.ID}");
            RemComp<ProjectilePhasePreventComponent>(uid);
            return;

        }

        projectile.raycasting = true;
        comp.start = _trans.GetWorldPosition(uid);
        comp.mapId = _trans.GetMapId(uid);
        /* Handled by  datafield in component.
        foreach(var (key , fixture) in Comp<FixturesComponent>(uid).Fixtures)
        {
            comp.relevantBitmasks |= fixture.CollisionLayer;
        };
        */
        if (processingBuckets.Last().items.Count >= raysPerThread)
        {
            processingBuckets.Add(new RaycastBucket());
        }
        processingBuckets.Last().items.Add((uid, comp, projectile));
        comp.containedAt = processingBuckets.Last();

    }

    private void OnRemove(EntityUid uid, ProjectilePhasePreventComponent comp, ref ComponentShutdown args)
    {
        if (!TryComp<ProjectileComponent>(uid, out var projectile))
        {
            sawLogs.Error($"Failed to remot entity without projectileComponent. Prototype : {MetaData(uid).EntityPrototype?.ID} [] ID : {uid}");
            return;
        }
        if (comp.containedAt is RaycastBucket bucket)
        {
            bucket.items.Remove((uid, comp, projectile));
            if (bucket.items.Count == 0 && processingBuckets.Count > 1)
            {
                processingBuckets.Remove(bucket);
            }
        }

    }


    public override void Update(float frametime)
    {
        Parallel.ForEach(processingBuckets, args =>
        {
            QueryFilter queryFilter = new QueryFilter();
            args.output.Clear();
            Vector2 worldPos = Vector2.Zero;
            foreach(var (owner,  phase, projectile) in args.items)
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
                    // whilst the raycast supports a filter function . i do not want to package variabiles in lambdas in bulk
                    if (projectile.Shooter == hit.Entity && projectile.IgnoreShooter)
                        continue;
                    if (projectile.Weapon == hit.Entity)
                        continue;
                    if (projectile.IgnoredEntities.Contains(hit.Entity))
                        continue;
                    // dont raise these. We cut some slack for the main thread by running it here.
                    if (projectile.Weapon is not null && Transform(projectile.Weapon.Value).GridUid == Transform(hit.Entity).GridUid)
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

        if (processingBuckets.Count > 3)
        {
            sawLogs.Info($"Processing {processingBuckets.Count} buckets with estimated bullet count of {processingBuckets.Count * raysPerThread}");
        }

        var count = 0;
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
                    count++;
                    RaiseLocalEvent(eventData.Item1.OurEntity, ref fEv, true);
                    RaiseLocalEvent(eventData.Item2.OurEntity, ref sEv, true);
                }
                catch (Exception e)
                {
                    sawLogs.Error(e.Message);
                }
            }
        }
        if (processingBuckets.Count > 3)
        {
            sawLogs.Info($"Processed {count} events on main-thread");
        }


    }
}
