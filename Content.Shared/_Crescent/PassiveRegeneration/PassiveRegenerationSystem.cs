using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Crescent.PassiveRegeneration;

/// <summary>
/// This handles...
/// </summary>
public sealed class PassiveRegenerationSystem : EntitySystem
{

    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _states = default!;

    EntityQuery<PassiveRegenerationComponent> _componentQuery;

    private float accumulator = 0f;


    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        accumulator += frameTime;
        if (accumulator > 10f)
        {
            accumulator = 0f;
            var query = EntityQueryEnumerator<PassiveRegenerationComponent>();
            while (query.MoveNext(out var uid, out var regenComp))
            {
                if (!TryComp<MobStateComponent>(uid, out var mobState))
                    continue;
                if (mobState.CurrentState == MobState.Dead)
                    continue;
                if(!TryComp<DamageableComponent>(uid, out var damageable))
                    continue;
                if(damageable.TotalDamage == 0)
                    continue;
                if(!TryComp<ThirstComponent>(uid, out var thirst) || !TryComp<HungerComponent>(uid, out var hunger))
                    continue;
                if(_hunger.IsHungerBelowState(uid, HungerThreshold.Okay, null, hunger) || _thirst.IsThirstBelowState(uid, ThirstThreshold.Parched, null, thirst))
                    continue;
                if (!_thirst.ModifyThirst(uid, thirst, -regenComp.thirstDrain) || !_hunger.ModifyHunger(uid, -regenComp.hungerDrain, hunger))
                    continue;
                _damageable.TryChangeDamage(uid, regenComp.HealPerTick, true);

            }
        }
    }
}
