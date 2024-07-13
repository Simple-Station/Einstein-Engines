using Content.Server.Electrocution;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Lightning;
using Content.Server.Lightning.Components;
using Content.Shared.Damage;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// The component allows lightning to strike this target. And determining the behavior of the target when struck by lightning.
/// </summary>
public sealed class LightningTargetSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightningTargetComponent, HitByLightningEvent>(OnHitByLightning);
    }

    private void OnHitByLightning(Entity<LightningTargetComponent> uid, ref HitByLightningEvent args)
    {
        if (args.Context.Electrocute(args.Discharge, args.Context))
        {
            _electrocutionSystem.TryDoElectrocution(uid, args.Context.Invoker, (int) Math.Round(args.Context.Damage(args.Discharge, args.Context), 0), TimeSpan.FromSeconds(5f), true);
        }

        if (args.Context.Explode(args.Discharge, args.Context))
        {
            DamageSpecifier damage = new();
            damage.DamageDict.Add("Structural", uid.Comp.DamageFromLightning);
            _damageable.TryChangeDamage(uid, damage, true);

            if (uid.Comp.LightningExplode)
            {
                _explosionSystem.QueueExplosion(
                    Transform(uid).MapPosition,
                    uid.Comp.ExplosionPrototype,
                    uid.Comp.TotalIntensity, uid.Comp.Dropoff,
                    uid.Comp.MaxTileIntensity,
                    canCreateVacuum: false);
            }
        }
    }
}
