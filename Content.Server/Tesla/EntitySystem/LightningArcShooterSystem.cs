using Content.Server.Administration.Commands;
using Content.Server.Lightning;
using Content.Shared.Lightning;
using Content.Server.Tesla.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// Fires electric arcs at surrounding objects.
/// </summary>
public sealed class LightningArcShooterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightningArcShooterComponent, MapInitEvent>(OnShooterMapInit);
    }

    private void OnShooterMapInit(EntityUid uid, LightningArcShooterComponent component, ref MapInitEvent args)
    {
        component.NextShootTime = _gameTiming.CurTime + TimeSpan.FromSeconds(component.ShootMaxInterval);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LightningArcShooterComponent>();
        while (query.MoveNext(out var uid, out var arcShooter))
        {
            if (arcShooter.NextShootTime > _gameTiming.CurTime)
                continue;

            ArcShoot(uid, arcShooter);
            var delay = TimeSpan.FromSeconds(_random.NextFloat(arcShooter.ShootMinInterval, arcShooter.ShootMaxInterval));
            arcShooter.NextShootTime += delay;
        }
    }

    private void ArcShoot(EntityUid uid, LightningArcShooterComponent component)
    {
        int lightningBolts = _random.Next(1, component.MaxBolts);
        int boltIterator = lightningBolts;
        int lightningArcs = _random.Next(lightningBolts, component.MaxArcs);

        int DynamicArcs(LightningContext context)
        {
            boltIterator -= 1;

            if (boltIterator > 0)
            {
                int diff = _random.Next(1, lightningArcs - boltIterator);
                lightningArcs -= diff;
                return diff;
            };

            return lightningArcs;
        }

        float DynamicCharge(LightningContext context)
        {
            return _random.Next(context.MaxArcs, (int) Math.Round((decimal) component.MaxArcs / lightningBolts, 0)) * 10000f;
        }

        LightningContext lightningContext = new LightningContext
        {
            ArcRange = (context) => component.ArcRadius,
            ArcForks = (context) =>
            {
                if (_random.NextFloat(0f, 1f) > component.ForkChance)
                    return 1;

                return _random.Next(2, Math.Max(2, component.MaxForks));
            },
            LightningPrototype = (discharge, context) => component.LightningPrototype.ToString(),
        };

        _lightning.ShootRandomLightnings(uid, component.BoltRadius, lightningBolts, lightningContext, dynamicArcs: DynamicArcs, dynamicCharge: DynamicCharge);
    }
}
