using System.Linq;
using Content.Server._Orehum.ME4TA.DWCCore.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Orehum.ME4TA.DWCCore;

public sealed class DWCCoreSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _time = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DWCCoreComponent, DWCCoreMobDeadEvent>(OnDWCCoreMobDeath);
        SubscribeLocalEvent<DWCCoreComponent, ComponentStartup>(OnDWCCoreStartup);
        SubscribeLocalEvent<DWCCoreMobComponent, MobStateChangedEvent>(OnMobState);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DWCCoreComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Mobs.Count >= comp.MaxSpawns)
                continue;
            if (comp.LastSpawn + TimeSpan.FromSeconds(comp.SpawnDelay) > _time.CurTime)
                continue;

            var xform = Transform(uid);
            var coords = xform.Coordinates;
            var newCoords = coords.Offset(_random.NextVector2(4));
            for (var i = 0; i < 100; i++)
            {
                var randVector = _random.NextVector2(4);
                newCoords = coords.Offset(randVector);
                if (!_lookup.GetEntitiesIntersecting(newCoords.ToMap(EntityManager, _transform), LookupFlags.Static).Any())
                {
                    break;
                }
            }
            var mob = Spawn(_random.Pick(comp.Spawns), newCoords);
            var mobComp = EnsureComp<DWCCoreMobComponent>(mob);
            mobComp.DWCCore = uid;
            comp.Mobs.Add(mob);
            comp.LastSpawn = _time.CurTime;
        }
    }

    private void OnDWCCoreStartup(EntityUid uid, DWCCoreComponent comp, ComponentStartup args)
    {
        comp.LastSpawn = _time.CurTime + TimeSpan.FromSeconds(5);
    }

    private void OnDWCCoreMobDeath(EntityUid uid, DWCCoreComponent comp, ref DWCCoreMobDeadEvent args)
    {
        comp.Mobs.Remove(args.Entity);
        comp.DefeatedMobs++;

        // John Shitcode
        if (comp.DefeatedMobs >= comp.MobsToDefeat)
        {
            comp.DestroyedWithMobs = true;
            _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = new Dictionary<string, FixedPoint2> {{ "Blunt", 1000 }} });
        }
    }

    private void OnMobState(EntityUid uid, DWCCoreMobComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!comp.DWCCore.HasValue)
            return;

        var ev = new DWCCoreMobDeadEvent(uid);
        RaiseLocalEvent(comp.DWCCore.Value, ref ev);
    }
}
