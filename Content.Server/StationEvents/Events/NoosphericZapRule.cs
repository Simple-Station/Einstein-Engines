using Content.Shared.GameTicking.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Popups;
using Content.Server.Psionics;
using Content.Server.StationEvents.Components;
using Content.Server.Stunnable;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;

namespace Content.Server.StationEvents.Events;

/// <summary>
/// Zaps everyone, rolling psionics and disorienting them
/// </summary>
internal sealed class NoosphericZapRule : StationEventSystem<NoosphericZapRuleComponent>
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly StunSystem _stunSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly PsionicsSystem _psionicsSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;

    protected override void Started(EntityUid uid, NoosphericZapRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<PsionicComponent, MobStateComponent>();

        while (query.MoveNext(out var psion, out var psionicComponent, out _))
        {
            if (!_mobStateSystem.IsAlive(psion) || HasComp<PsionicInsulationComponent>(psion))
                continue;

            _stunSystem.TryParalyze(psion, TimeSpan.FromSeconds(component.StunDuration), false);
            _statusEffectsSystem.TryAddStatusEffect(psion, "Stutter", TimeSpan.FromSeconds(component.StutterDuration), false, "StutteringAccent");

            if (!psionicComponent.CanReroll)
            {
                psionicComponent.CanReroll = true;
                _popupSystem.PopupEntity(Loc.GetString("noospheric-zap-seize-potential-regained"), psion, psion, Shared.Popups.PopupType.LargeCaution);
            }
            else
            {
                _psionicsSystem.RollPsionics(psion, psionicComponent, true, component.PowerRerollMultiplier);
                _popupSystem.PopupEntity(Loc.GetString("noospheric-zap-seize"), psion, psion, Shared.Popups.PopupType.LargeCaution);
            }
        }
    }
}
