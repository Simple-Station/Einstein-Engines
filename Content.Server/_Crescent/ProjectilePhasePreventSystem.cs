using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Server.Lightning;
using Content.Shared._Crescent;
using Content.Shared._Lavaland.Weapons;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Threading;


/// <summary>
///  Written by MLGTASTICa/SPCR 2025 for Hullrot EE
///  This was initially using RobustParallel Manager, but the implementation is horrendous for this kind of task (threadPooling (fake c# threads) instead of high-performance single CPU threads)
///  This system is expected to be ran on objects that DO not have any HARD-FIXTURES. As in all-collision events are handled only by this and not by physics due to actual body collision.
/// </summary>
///
[ByRefEvent]
public class HullrotBulletHitEvent : EntityEventArgs
{
    public EntityUid selfEntity;
    public EntityUid hitEntity;
    public Fixture targetFixture = default!;
    public Fixture selfFixture = default!;
    public string selfFixtureKey = string.Empty;
    public string targetFixtureKey = string.Empty;


}
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
        public List<HullrotBulletHitEvent> output = new();

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
            args.output.Clear();
            Vector2 worldPos;
            foreach(var (owner,  phase, projectile) in args.items)
            {
                // will be removed through events. Just skip for now
                if (TerminatingOrDeleted(owner))
                    continue;
                if (Deleted(owner)) //.2 2025 - MLG said we should do this because it fixes the metadata error
                    continue;         // its trying to run this shit on a deleting entity so
                worldPos = _trans.GetWorldPosition(owner);
                if ((worldPos - phase.start).IsLengthZero())
                    continue;
                CollisionRay ray = new CollisionRay(phase.start, (worldPos - phase.start).Normalized(), phase.relevantBitmasks);
                var rayLength = (worldPos - phase.start).Length();
                phase.start = worldPos;
                var bulletPhysics = physQuery.GetComponent(owner);
                var bulletFixtures = fixtureQuery.GetComponent(owner);
                var bulletString = bulletFixtures.Fixtures.Keys.First();
                var checkUid = EntityUid.Invalid;
                if (!TryComp<TransformComponent>(projectile.Weapon, out var projectileWeaponTransform))
                    continue;
                if (projectile.Weapon is not null && projectileWeaponTransform.GridUid is not null)
                    checkUid = projectileWeaponTransform.GridUid!.Value;
                foreach (var hit in _phys.IntersectRay(_trans.GetMapId(owner), ray,rayLength, projectile.Weapon, false))
                {
                    // whilst the raycast supports a filter function . i do not want to package variabiles in lambdas in bulk
                    if (projectile.Shooter == hit.HitEntity && projectile.IgnoreShooter)
                        continue;
                    if (projectile.IgnoredEntities.Contains(hit.HitEntity))
                        continue;
                    if (!TryComp<TransformComponent>(hit.HitEntity, out var hitTransform))
                        continue;
                    // dont raise these. We cut some slack for the main thread by running it here.
                    if (hitTransform.GridUid is not null && checkUid == hitTransform.GridUid && projectile.IgnoreWeaponGrid)
                        continue;
                    var targetPhysics = physQuery.GetComponent(hit.HitEntity);
                    var targetFixtures = fixtureQuery.GetComponent(hit.HitEntity);
                    var targetString = targetFixtures.Fixtures.Keys.First();
                    // i hate how verbose this is. - SPCR 2025
                    var bulletEvent = new HullrotBulletHitEvent()
                    {
                        selfEntity = owner,
                        hitEntity = hit.HitEntity,
                        selfFixtureKey = bulletString,
                        targetFixture = targetFixtures.Fixtures.Values.First(),
                        targetFixtureKey = targetString,
                    };

                    args.output.Add(bulletEvent);
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
                if (TerminatingOrDeleted(eventData.selfEntity) || TerminatingOrDeleted(eventData.hitEntity))
                    continue;
                var fEv = eventData;
                try
                {
                    count++;
                    RaiseLocalEvent(eventData.selfEntity, ref fEv, true);
                    //Logger.Debug($"Raised event on {MetaData(eventData.selfEntity).EntityName}"); //dont think we need this anymore .2 | 2025
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
