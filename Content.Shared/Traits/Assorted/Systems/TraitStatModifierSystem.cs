using Content.Shared.Contests;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Mood;
using Robust.Shared.Random;
using Robust.Shared.GameObjects;
using Content.Shared.FixedPoint;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed class CritModifierChangedEvent : EntityEventArgs { }

public sealed partial class TraitStatModifierSystem : EntitySystem
{
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CritModifierComponent, ComponentStartup>(OnCritStartup);
        SubscribeLocalEvent<CritModifierComponent, CritModifierChangedEvent>(OnCritChanged);
        SubscribeLocalEvent<CritModifierComponent, ComponentShutdown>(OnCritShutdown);
        SubscribeLocalEvent<DeadModifierComponent, ComponentStartup>(OnDeadStartup);
        SubscribeLocalEvent<StaminaCritModifierComponent, ComponentStartup>(OnStaminaCritStartup);
        SubscribeLocalEvent<AdrenalineComponent, GetMeleeDamageEvent>(OnAdrenalineGetMeleeDamage);
        SubscribeLocalEvent<AdrenalineComponent, GetThrowingDamageEvent>(OnAdrenalineGetThrowingDamage);
        SubscribeLocalEvent<PainToleranceComponent, GetMeleeDamageEvent>(OnPainToleranceGetMeleeDamage);
        SubscribeLocalEvent<PainToleranceComponent, GetThrowingDamageEvent>(OnPainToleranceGetThrowingDamage);
        SubscribeLocalEvent<ManicComponent, OnSetMoodEvent>(OnManicMood);
        SubscribeLocalEvent<MercurialComponent, OnSetMoodEvent>(OnMercurialMood);
    }

    private static FixedPoint2 F2(float v) => FixedPoint2.New(v);
    private static float F2f(FixedPoint2 v) => (float) v;

    private void ReapplyCrit(EntityUid uid, CritModifierComponent component, MobThresholdsComponent threshold)
    {
        FixedPoint2 baseF2;

        if (component.OriginalCritThreshold != 0f)
        {
            baseF2 = F2(component.OriginalCritThreshold);
        }
        else
        {
            baseF2 = _threshold.GetThresholdForState(uid, Mobs.MobState.Critical, threshold);
            if (baseF2 == FixedPoint2.Zero)
                return;
        }
        var modsF2 = F2(component.CritThresholdModifier + component.ChemActive);

        var newTotalF2 = baseF2 + modsF2;
        _threshold.SetMobStateThreshold(uid, newTotalF2, Mobs.MobState.Critical);
    }

    private void OnCritStartup(EntityUid uid, CritModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<MobThresholdsComponent>(uid, out var threshold))
            return;

        var currentF2 = _threshold.GetThresholdForState(uid, Mobs.MobState.Critical, threshold);
        if (currentF2 != FixedPoint2.Zero)
            component.OriginalCritThreshold = F2f(currentF2);

        ReapplyCrit(uid, component, threshold);
    }

    private void OnCritChanged(EntityUid uid, CritModifierComponent component, CritModifierChangedEvent args)
    {
        if (!TryComp<MobThresholdsComponent>(uid, out var threshold))
            return;

        ReapplyCrit(uid, component, threshold);
    }

    private void OnCritShutdown(EntityUid uid, CritModifierComponent component, ComponentShutdown args)
    {
        if (!TryComp<MobThresholdsComponent>(uid, out var threshold))
            return;

        if (component.OriginalCritThreshold != 0f)
        {
            _threshold.SetMobStateThreshold(uid, F2(component.OriginalCritThreshold), Mobs.MobState.Critical);
        }
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

    private void OnManicMood(EntityUid uid, ManicComponent component, ref OnSetMoodEvent args) =>
        args.MoodChangedAmount *= _random.NextFloat(component.LowerMultiplier, component.UpperMultiplier);

    private void OnMercurialMood(EntityUid uid, MercurialComponent component, ref OnSetMoodEvent args) =>
        args.MoodOffset += _random.NextFloat(component.LowerMood, component.UpperMood);
}
