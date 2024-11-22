using Content.Shared.Damage.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Damage.Systems;

public sealed partial class StaminaSystem
{
    private void InitializeSlowdown()
    {
        SubscribeLocalEvent<StaminaComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshModifiers);
    }

    private void OnRefreshModifiers(Entity<StaminaComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var damage = ent.Comp.StaminaDamage;
        var threshold = ent.Comp.SlowdownThresholdFactor * ent.Comp.CritThreshold;
        if (damage < threshold)
            return;

        var factor = ent.Comp.SlowdownMultiplier * damage / ent.Comp.CritThreshold;
        args.ModifySpeed(factor, factor);
    }
}
