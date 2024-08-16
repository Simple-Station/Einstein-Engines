﻿using Content.Server.Popups;
using Content.Shared._EstacaoPirata.EmitBuzzWhileDamaged;
using Content.Shared.Audio;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._EstacaoPirata.EmitBuzzOnCrit;

/// <summary>
/// This handles the buzzing popup and sound of a silicon based race when it is pretty damaged.
/// </summary>
public sealed class EmitBuzzWhileDamagedSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EmitBuzzWhileDamagedComponent, BodyComponent>();

        while (query.MoveNext(out var uid, out var emitBuzzOnCritComponent, out var body))
        {

            if (_mobState.IsDead(uid))
                continue;
            if (!_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var threshold) ||
                !TryComp(uid, out DamageableComponent? damageableComponent))
                continue;

            if (damageableComponent.TotalDamage < (threshold/2))
                continue;


            emitBuzzOnCritComponent.AccumulatedFrametime += frameTime;

            if (emitBuzzOnCritComponent.AccumulatedFrametime < emitBuzzOnCritComponent.CycleDelay)
                continue;
            emitBuzzOnCritComponent.AccumulatedFrametime -= emitBuzzOnCritComponent.CycleDelay;


            // start buzzing
            if (_gameTiming.CurTime >= emitBuzzOnCritComponent.LastBuzzPopupTime + emitBuzzOnCritComponent.BuzzPopupCooldown)
            {
                emitBuzzOnCritComponent.LastBuzzPopupTime = _gameTiming.CurTime;
                _popupSystem.PopupEntity(Loc.GetString("silicon-behavior-buzz"), uid);
                Spawn("EffectSparks", Transform(uid).Coordinates);
                _audio.PlayPvs(emitBuzzOnCritComponent.Sound, uid, AudioHelpers.WithVariation(0.05f, _robustRandom));
            }
        }
    }

}
