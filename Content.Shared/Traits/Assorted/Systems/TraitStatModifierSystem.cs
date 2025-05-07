using Content.Shared.Contests;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Damage.Events;
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
        SubscribeLocalEvent<AdrenalineComponent, GetMeleeDamageEvent>(OnAdrenalineGetMeleeDamage);
        SubscribeLocalEvent<AdrenalineComponent, GetThrowingDamageEvent>(OnAdrenalineGetThrowingDamage);
        SubscribeLocalEvent<PainToleranceComponent, GetMeleeDamageEvent>(OnPainToleranceGetMeleeDamage);
        SubscribeLocalEvent<PainToleranceComponent, GetThrowingDamageEvent>(OnPainToleranceGetThrowingDamage);
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

    private void OnAdrenalineGetMeleeDamage(EntityUid uid, AdrenalineComponent component, ref GetMeleeDamageEvent args)
    {
        args.Damage *= GetAdrenalineMultiplier(uid, component);
    }

    private void OnAdrenalineGetThrowingDamage(EntityUid uid, AdrenalineComponent component, ref GetThrowingDamageEvent args)
    {
        args.Damage *= GetAdrenalineMultiplier(uid, component);
    }

    private float GetAdrenalineMultiplier(EntityUid uid, AdrenalineComponent component)
    {
        var modifier = _contests.HealthContest(uid, component.BypassClamp, component.RangeModifier);
        return component.Inverse ? 1 / modifier : modifier;
    }

    private void OnPainToleranceGetMeleeDamage(EntityUid uid, PainToleranceComponent component, ref GetMeleeDamageEvent args)
    {
        args.Damage *= GetPainToleranceMultiplier(uid, component);
    }

    private void OnPainToleranceGetThrowingDamage(EntityUid uid, PainToleranceComponent component, ref GetThrowingDamageEvent args)
    {
        args.Damage *= GetPainToleranceMultiplier(uid, component);
    }

    private float GetPainToleranceMultiplier(EntityUid uid, PainToleranceComponent component)
    {
        var modifier = _contests.StaminaContest(uid, component.BypassClamp, component.RangeModifier);
        return component.Inverse ? 1 / modifier : modifier;
    }
}
