using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Psionics.Abilities;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using Content.Shared.Psionics.Events;

namespace Content.Server.Psionics.Abilities
{
    public sealed class MetapsionicPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MetapsionicPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<MetapsionicPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MetapsionicPowerComponent, WideMetapsionicPowerActionEvent>(OnWidePowerUsed);
            SubscribeLocalEvent<FocusedMetapsionicPowerActionEvent>(OnFocusedPowerUsed);
            SubscribeLocalEvent<MetapsionicPowerComponent, FocusedMetapsionicDoAfterEvent>(OnDoAfter);
        }

        private void OnInit(EntityUid uid, MetapsionicPowerComponent component, ComponentInit args)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);
            if (!TryComp(uid, out ActionsComponent? comp))
                return;
            _actions.AddAction(uid, ref component.ActionWideMetapsionicEntity, component.ActionWideMetapsionic, component: comp);
            _actions.AddAction(uid, ref component.ActionFocusedMetapsionicEntity, component.ActionFocusedMetapsionic, component: comp);
            UpdateActions(uid, component, psionic);

            psionic.ActivePowers.Add(component);
            psionic.PsychicFeedback.Add(component.MetapsionicFeedback);
            psionic.Amplification += 0.1f;
            psionic.Dampening += 0.5f;
        }

        private void UpdateActions(EntityUid uid, MetapsionicPowerComponent? component = null, PsionicComponent? psionic = null)
        {
            if (!Resolve(uid, ref component) || !Resolve(uid, ref psionic)
            || !_actions.TryGetActionData(component.ActionWideMetapsionicEntity, out var actionData))
                return;

            if (actionData is { UseDelay: not null })
            {
                _actions.SetCooldown(component.ActionWideMetapsionicEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));
                _actions.SetCooldown(component.ActionFocusedMetapsionicEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));
            }
        }

        private void OnShutdown(EntityUid uid, MetapsionicPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.ActionWideMetapsionicEntity);
            _actions.RemoveAction(uid, component.ActionFocusedMetapsionicEntity);

            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.MetapsionicFeedback);
                psionic.Amplification -= 0.1f;
                psionic.Dampening -= 0.5f;
            }
        }

        private void OnWidePowerUsed(EntityUid uid, MetapsionicPowerComponent component, WideMetapsionicPowerActionEvent args)
        {
            if (HasComp<PsionicInsulationComponent>(uid))
                return;

            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            foreach (var entity in _lookup.GetEntitiesInRange(uid, component.Range))
            {
                if (HasComp<PsionicComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity) &&
                    !(HasComp<ClothingGrantPsionicPowerComponent>(entity) && Transform(entity).ParentUid == uid))
                {
                    _popups.PopupEntity(Loc.GetString("metapsionic-pulse-success"), uid, uid, PopupType.LargeCaution);
                    args.Handled = true;
                    return;
                }
            }
            _popups.PopupEntity(Loc.GetString("metapsionic-pulse-failure"), uid, uid, PopupType.Large);
            _psionics.LogPowerUsed(uid, "metapsionic pulse", psionic, 2, 4);
            UpdateActions(uid, component, psionic);
            args.Handled = true;
        }

        private void OnFocusedPowerUsed(FocusedMetapsionicPowerActionEvent args)
        {
            if (!TryComp<PsionicComponent>(args.Performer, out var psionic))
                return;

            if (HasComp<PsionicInsulationComponent>(args.Target))
                return;

            if (!TryComp<MetapsionicPowerComponent>(args.Performer, out var component))
                return;

            var ev = new FocusedMetapsionicDoAfterEvent(_gameTiming.CurTime);

            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.Performer, component.UseDelay - psionic.Amplification, ev, args.Performer, args.Target, args.Performer)
            {
                BlockDuplicate = true,
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
                BreakOnDamage = true,
            }, out var doAfterId);

            component.DoAfter = doAfterId;

            _popups.PopupEntity(Loc.GetString("focused-metapsionic-pulse-begin", ("entity", args.Target)), args.Performer, PopupType.Medium);

            _audioSystem.PlayPvs(component.SoundUse, args.Performer, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
            _psionics.LogPowerUsed(args.Performer, "focused metapsionic pulse", psionic, 3, 6);
            args.Handled = true;

            UpdateActions(args.Performer, component, psionic);
        }

        private void OnDoAfter(EntityUid uid, MetapsionicPowerComponent component, FocusedMetapsionicDoAfterEvent args)
        {
            component.DoAfter = null;

            if (args.Target == null || args.Cancelled)
                return;

            if (TryComp<MindSwappedComponent>(args.Target, out var swapped))
            {
                _popups.PopupEntity(Loc.GetString(swapped.MindSwappedFeedback, ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                return;
            }

            if (args.Target == uid)
            {
                _popups.PopupEntity(Loc.GetString("metapulse-self", ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                return;
            }

            if (!HasComp<PotentialPsionicComponent>(args.Target))
            {
                _popups.PopupEntity(Loc.GetString("no-powers", ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                return;
            }

            if (HasComp<PotentialPsionicComponent>(args.Target) & !HasComp<PsionicComponent>(args.Target))
            {
                _popups.PopupEntity(Loc.GetString("psychic-potential", ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                return;
            }

            if (TryComp<PsionicComponent>(args.Target, out var psychic))
            {
                foreach (var psychicFeedback in psychic.PsychicFeedback)
                {
                    _popups.PopupEntity(Loc.GetString(psychicFeedback, ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                }
            }
        }
    }
}
