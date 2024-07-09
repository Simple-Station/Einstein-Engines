using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Stunnable;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Psionics.Events;
using Content.Shared.Actions.Events;
using Robust.Shared.Network;
using Robust.Shared.Audio.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Throwing;
using Robust.Shared.Timing;

namespace Content.Shared.Psionics.Abilities
{
    public sealed class PsionicInvisibilityPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedStunSystem _stunSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly SharedStealthSystem _stealth = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, PsionicInvisibilityPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<RemovePsionicInvisibilityOffPowerActionEvent>(OnPowerOff);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentInit>(OnStart);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentShutdown>(OnEnd);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, AttackAttemptEvent>(OnAttackAttempt);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ShotAttemptedEvent>(OnShootAttempt);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ThrowAttemptEvent>(OnThrowAttempt);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, PsionicInsulationEvent>(OnInsulated);
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, PsionicInvisibilityTimerEvent>(OnDoAfter);
        }

        private void OnInit(EntityUid uid, PsionicInvisibilityPowerComponent component, ComponentInit args)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);
            _actions.AddAction(uid, ref component.PsionicInvisibilityActionEntity, component.PsionicInvisibilityActionId);
            _actions.TryGetActionData(component.PsionicInvisibilityActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.SetCooldown(component.PsionicInvisibilityActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));

            psionic.ActivePowers.Add(component);
            psionic.PsychicFeedback.Add(component.InvisibilityFeedback);
            psionic.Amplification += 0.5f;
        }

        private void OnShutdown(EntityUid uid, PsionicInvisibilityPowerComponent component, ComponentShutdown args)
        {
            RemComp<PsionicInvisibilityUsedComponent>(uid);
            RemComp<PsionicallyInvisibleComponent>(uid);
            _actions.RemoveAction(uid, component.PsionicInvisibilityActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.InvisibilityFeedback);
                psionic.Amplification -= 0.5f;
            }
        }

        private void OnPowerUsed(EntityUid uid, PsionicInvisibilityPowerComponent component, PsionicInvisibilityPowerActionEvent args)
        {
            if (!_psionics.CheckCanSelfCast(uid, out var psionic))
                return;

            var ev = new PsionicInvisibilityTimerEvent(_gameTiming.CurTime);
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.UseTimer, ev, uid) { Hidden = true };
            if (_doAfterSystem.TryStartDoAfter(doAfterArgs, out var doAfterId))
            {
                ToggleInvisibility(args.Performer);
                if (TryComp<PsionicInvisibilityUsedComponent>(uid, out var invis))
                {
                    _actions.AddAction(uid, ref invis.PsionicInvisibilityUsedActionEntity, invis.PsionicInvisibilityUsedActionId);
                    invis.DoAfter = doAfterId;
                }

                _psionics.LogPowerUsed(uid, "psionic invisibility", psionic, 8, 12);
                args.Handled = true;
            }

            _actions.TryGetActionData(component.PsionicInvisibilityActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.SetCooldown(component.PsionicInvisibilityActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));
        }

        private void OnPowerOff(RemovePsionicInvisibilityOffPowerActionEvent args)
        {
            ToggleInvisibility(args.Performer);
            args.Handled = true;
        }

        private void OnStart(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentInit args)
        {
            EnsureComp<PsionicallyInvisibleComponent>(uid);
            var stealth = EnsureComp<StealthComponent>(uid);
            _stealth.SetVisibility(uid, 0.66f, stealth);

            if (_net.IsServer)
                _audio.PlayPvs("/Audio/Effects/toss.ogg", uid);

        }

        private void OnEnd(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentShutdown args)
        {
            if (Terminating(uid))
                return;

            _doAfterSystem.Cancel(component.DoAfter);
            RemComp<PsionicallyInvisibleComponent>(uid);
            RemComp<StealthComponent>(uid);

            if (_net.IsServer)
                _audio.PlayPvs("/Audio/Effects/toss.ogg", uid);

            _actions.RemoveAction(uid, component.PsionicInvisibilityUsedActionEntity);
            DirtyEntity(uid);
        }

        private void OnAttackAttempt(EntityUid uid, PsionicInvisibilityUsedComponent component, AttackAttemptEvent args)
        {
            RemComp<PsionicInvisibilityUsedComponent>(uid);
        }

        private void OnShootAttempt(EntityUid uid, PsionicInvisibilityUsedComponent component, ShotAttemptedEvent args)
        {
            RemComp<PsionicInvisibilityUsedComponent>(uid);
        }

        private void OnThrowAttempt(EntityUid uid, PsionicInvisibilityUsedComponent component, ThrowAttemptEvent args)
        {
            RemComp<PsionicInvisibilityUsedComponent>(uid);
        }
        private void OnDamageChanged(EntityUid uid, PsionicInvisibilityUsedComponent component, DamageChangedEvent args)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            if (!args.DamageIncreased)
                return;

            ToggleInvisibility(uid);
            _stunSystem.TryParalyze(uid, TimeSpan.FromSeconds(4f / psionic.Dampening + psionic.Amplification), false);
        }
        public void ToggleInvisibility(EntityUid uid)
        {
            if (!HasComp<PsionicInvisibilityUsedComponent>(uid))
            {
                EnsureComp<PsionicInvisibilityUsedComponent>(uid);
            } else
            {
                RemComp<PsionicInvisibilityUsedComponent>(uid);
            }
        }

        private void OnDoAfter(EntityUid uid, PsionicInvisibilityPowerComponent component, PsionicInvisibilityTimerEvent args)
        {
            if (!args.Cancelled)
                RemComp<PsionicInvisibilityUsedComponent>(uid);
        }

        private void OnInsulated(EntityUid uid, PsionicInvisibilityUsedComponent component, PsionicInsulationEvent args)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            RemComp<PsionicInvisibilityUsedComponent>(uid);
            _stunSystem.TryParalyze(uid, TimeSpan.FromSeconds(4f / psionic.Dampening + psionic.Amplification), false);
        }
    }
}
