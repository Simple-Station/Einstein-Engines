using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.Assorted;
using Content.Shared.Damage.Events;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage.Components;
using Robust.Shared.Random;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed partial class TraitStatModifierSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeadModifierComponent, ComponentStartup>(OnDeadStartup);
        SubscribeLocalEvent<CritModifierComponent, ComponentStartup>(OnCritStartup);
    }

    private void OnDeadStartup(EntityUid uid, DeadModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<MobThresholdsComponent>(uid, out var threshold))
            return;

        var deadThreshold = _threshold.GetThresholdForState(uid, Mobs.MobState.Dead, threshold);
        if (deadThreshold != 0)
            _threshold.SetMobStateThreshold(uid, deadThreshold + component.DeadThresholdModifier, Mobs.MobState.Dead);
    }

    private void OnCritStartup(EntityUid uid, CritModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<MobThresholdsComponent>(uid, out var threshold))
            return;

        var critThreshold = _threshold.GetThresholdForState(uid, Mobs.MobState.Critical, threshold);
        if (critThreshold != 0)
            _threshold.SetMobStateThreshold(uid, critThreshold + component.CritThresholdModifier, Mobs.MobState.Critical);
    }

}
