using Content.Shared.Contests;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage.Components;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed partial class TraitStatModifierSystem : EntitySystem
{
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CritModifierComponent, ComponentStartup>(OnCritStartup);
        SubscribeLocalEvent<DeadModifierComponent, ComponentStartup>(OnDeadStartup);
        SubscribeLocalEvent<StaminaCritModifierComponent, ComponentStartup>(OnStaminaCritStartup);
        SubscribeLocalEvent<AdrenalineComponent, GetMeleeDamageEvent>(OnAdrenalineGetDamage);
        SubscribeLocalEvent<PainToleranceComponent, GetMeleeDamageEvent>(OnPainToleranceGetDamage);
    }

    private void OnCritStartup(EntityUid uid, CritModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<MobThresholdsComponent>(uid, out var threshold))
            return;

        var critThreshold = _threshold.GetThresholdForState(uid, Mobs.MobState.Critical, threshold);
        if (critThreshold != 0)
            _threshold.SetMobStateThreshold(uid, critThreshold + component.CritThresholdModifier, Mobs.MobState.Critical);
    }

    private void OnDeadStartup(EntityUid uid, DeadModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<MobThresholdsComponent>(uid, out var threshold))
            return;

        var deadThreshold = _threshold.GetThresholdForState(uid, Mobs.MobState.Dead, threshold);
        if (deadThreshold != 0)
            _threshold.SetMobStateThreshold(uid, deadThreshold + component.DeadThresholdModifier, Mobs.MobState.Dead);
    }

    private void OnStaminaCritStartup(EntityUid uid, StaminaCritModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<StaminaComponent>(uid, out var stamina))
            return;

        stamina.CritThreshold += component.CritThresholdModifier;
    }

    private void OnAdrenalineGetDamage(EntityUid uid, AdrenalineComponent component, ref GetMeleeDamageEvent args)
    {
        var modifier = _contests.HealthContest(uid, component.BypassClamp, component.RangeModifier);
        args.Damage *= component.Inverse ? 1 / modifier : modifier;
    }

    private void OnPainToleranceGetDamage(EntityUid uid, PainToleranceComponent component, ref GetMeleeDamageEvent args)
    {
        var modifier = _contests.StaminaContest(uid, component.BypassClamp, component.RangeModifier);
        args.Damage *= component.Inverse ? 1 / modifier : modifier;
    }
}
