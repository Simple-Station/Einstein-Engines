using Content.Server.Electrocution;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Lightning;
using Content.Server.Lightning.Components;
using Content.Shared.Damage;
using Content.Shared.Electrocution;

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

        SubscribeLocalEvent<LightningTargetComponent, LightningStageEvent>(OnLightningStage);
        SubscribeLocalEvent<LightningTargetComponent, LightningEffectEvent>(OnLightningEffect);
    }

    private void OnLightningStage(Entity<LightningTargetComponent> uid, ref LightningStageEvent args)
    {
        // Reduce the number of lightning jumps based on lightning modifiers
        args.Context.MaxArcs -= uid.Comp.LightningArcReduction;
    }
    private void OnLightningEffect(Entity<LightningTargetComponent> uid, ref LightningEffectEvent args)
    {
        // Reduce the residual charge of lighting based on lightning modifiers
        args.Context.Charge -= uid.Comp.LightningChargeReduction;
        args.Context.Charge *= uid.Comp.LightningChargeMultiplier;

        // Deal damage as specified by lightning
        if (!args.Context.DamageIgnoreInsulation(args.Discharge, args.Context) || !TryComp<InsulatedComponent>(uid, out var _))
            _damageable.TryChangeDamage(uid, args.Context.Damage(args.Discharge, args.Context), true);

        // Attempt to electrocute the target
        if (args.Context.Electrocute(args.Discharge, args.Context))
        {
            _electrocutionSystem.TryDoElectrocution(uid, args.Context.Invoker, 0, TimeSpan.FromSeconds(5f), true, ignoreInsulation: args.Context.ElectrocuteIgnoreInsulation(args.Discharge, args.Context));
        }

        // Attempt to explode the target, provided that they are explosives
        if (args.Context.Explode(args.Discharge, args.Context))
        {
            if (!TryComp<ExplosiveComponent>(uid, out var bomb))
                return;

            // don't delete the target because it looks jarring, the explosion destroys most things anyhow
            _explosionSystem.TriggerExplosive(uid, bomb, false);
        }
    }
}
