using Content.Server.Electrocution;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Lightning;
using Content.Server.Lightning.Components;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// The component allows lightning to strike this target. And determining the behavior of the target when struck by lightning.
/// </summary>
public sealed class LightningTargetSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightningTargetComponent, LightningStageEvent>(OnLightningStage);
        SubscribeLocalEvent<LightningTargetComponent, LightningEffectEvent>(OnLightningEffect);
    }

    private void OnLightningStage(EntityUid uid, LightningTargetComponent comp, LightningStageEvent args)
    {
        // Reduce the number of lightning jumps based on lightning modifiers
        args.Context.MaxArcs -= comp.LightningArcReduction;
    }

    private void OnLightningEffect(EntityUid uid, LightningTargetComponent comp, LightningEffectEvent args)
    {
        // Reduce the residual charge of lighting based on lightning modifiers
        args.Context.Charge -= comp.LightningChargeReduction;
        args.Context.Charge *= comp.LightningChargeMultiplier;

        // Attempt to electrocute the target
        if (args.Context.Electrocute(args.Discharge, args.Context))
        {
            _electrocutionSystem.TryDoElectrocution(uid, args.Context.Invoker, args.Context.ElectrocuteDamage(args.Discharge, args.Context), TimeSpan.FromSeconds(5f), true, ignoreInsulation: args.Context.ElectrocuteIgnoreInsulation(args.Discharge, args.Context));
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
