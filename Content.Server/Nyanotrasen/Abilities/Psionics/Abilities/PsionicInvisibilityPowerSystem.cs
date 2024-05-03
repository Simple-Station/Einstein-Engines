using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Damage;
using Content.Shared.Stunnable;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Server.Psionics;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicInvisibilityPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedStunSystem _stunSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly SharedStealthSystem _stealth = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, PsionicInvisibilityPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<PsionicInvisibilityPowerOffActionEvent>(OnPowerOff);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentInit>(OnStart);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentShutdown>(OnEnd);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, DamageChangedEvent>(OnDamageChanged);
        }

        private void OnInit(EntityUid uid, PsionicInvisibilityPowerComponent component, ComponentInit args)
        {
            if (!_prototypeManager.TryIndex<InstantActionPrototype>("PsionicInvisibility", out var invis))
                return;

            component.PsionicInvisibilityPowerAction = new InstantAction(invis);
            if (invis.UseDelay != null)
                component.PsionicInvisibilityPowerAction.Cooldown = (_gameTiming.CurTime, _gameTiming.CurTime + (TimeSpan) invis.UseDelay);
            _actions.AddAction(uid, component.PsionicInvisibilityPowerAction, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.PsionicInvisibilityPowerAction;
        }

        private void OnShutdown(EntityUid uid, PsionicInvisibilityPowerComponent component, ComponentShutdown args)
        {
            if (_prototypeManager.TryIndex<InstantActionPrototype>("PsionicInvisibility", out var invis))
                _actions.RemoveAction(uid, new InstantAction(invis), null);
        }

        private void OnPowerUsed(EntityUid uid, PsionicInvisibilityPowerComponent component, PsionicInvisibilityPowerActionEvent args)
        {
            if (HasComp<PsionicInvisibilityUsedComponent>(uid))
                return;

            ToggleInvisibility(args.Performer);

            if (_prototypeManager.TryIndex<InstantActionPrototype>("PsionicInvisibilityOff", out var invis))
                _actions.AddAction(args.Performer, new InstantAction(invis), null);

            _psionics.LogPowerUsed(uid, "psionic invisibility");
            args.Handled = true;
        }

        private void OnPowerOff(PsionicInvisibilityPowerOffActionEvent args)
        {
            if (!HasComp<PsionicInvisibilityUsedComponent>(args.Performer))
                return;

            ToggleInvisibility(args.Performer);
            args.Handled = true;
        }

        private void OnStart(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentInit args)
        {
            EnsureComp<PsionicallyInvisibleComponent>(uid);
            EnsureComp<PacifiedComponent>(uid);
            var stealth = EnsureComp<StealthComponent>(uid);
            _stealth.SetVisibility(uid, 0.66f, stealth);
            SoundSystem.Play("/Audio/Effects/toss.ogg", Filter.Pvs(uid), uid);

        }

        private void OnEnd(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentShutdown args)
        {
            if (Terminating(uid))
                return;

            RemComp<PsionicallyInvisibleComponent>(uid);
            RemComp<PacifiedComponent>(uid);
            RemComp<StealthComponent>(uid);
            SoundSystem.Play("/Audio/Effects/toss.ogg", Filter.Pvs(uid), uid);

            if (_prototypeManager.TryIndex<InstantActionPrototype>("PsionicInvisibilityOff", out var invis))
                _actions.RemoveAction(uid, new InstantAction(invis), null);

            _stunSystem.TryParalyze(uid, TimeSpan.FromSeconds(8), false);
            Dirty(uid);
        }

        private void OnDamageChanged(EntityUid uid, PsionicInvisibilityUsedComponent component, DamageChangedEvent args)
        {
            if (!args.DamageIncreased)
                return;

            ToggleInvisibility(uid);
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
    }

    public sealed class PsionicInvisibilityPowerActionEvent : InstantActionEvent {}
    public sealed class PsionicInvisibilityPowerOffActionEvent : InstantActionEvent {}
}
