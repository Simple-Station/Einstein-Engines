using Content.Server.Body.Systems;
using Content.Server.Body.Components;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Popups;
using Content.Shared.Psionics.Abilities;
using Content.Shared.Actions.Events;
using Content.Shared.FixedPoint;

namespace Content.Server.Psionics.Abilities
{
    public sealed class MassSleepPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RegenerativeStasisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<RegenerativeStasisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<RegenerativeStasisPowerComponent, RegenerativeStasisPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, RegenerativeStasisPowerComponent component, ComponentInit args)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);
            _actions.AddAction(uid, ref component.RegenerativeStasisActionEntity, component.RegenerativeStasisActionId);
            _actions.TryGetActionData(component.RegenerativeStasisActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.SetCooldown(component.RegenerativeStasisActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));

            psionic.ActivePowers.Add(component);
            psionic.PsychicFeedback.Add(component.RegenerativeStasisFeedback);
            psionic.Amplification += 0.5f;
            psionic.Dampening += 0.5f;
        }

        private void OnShutdown(EntityUid uid, RegenerativeStasisPowerComponent component, ComponentShutdown args)
        {
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.RegenerativeStasisFeedback);
                psionic.Amplification -= 0.5f;
                psionic.Dampening -= 0.5f;
            }
            _actions.RemoveAction(uid, component.RegenerativeStasisActionEntity);
        }

        private void OnPowerUsed(EntityUid uid, RegenerativeStasisPowerComponent component, RegenerativeStasisPowerActionEvent args)
        {
            if (TryComp<PsionicComponent>(uid, out var psionic)
            && !HasComp<PsionicInsulationComponent>(uid)
            && !HasComp<PsionicInsulationComponent>(args.Target)
            && TryComp<BloodstreamComponent>(args.Target, out var stream))
            {
                var solution = new Solution();
                solution.AddReagent("PsionicRegenerationEssence", FixedPoint2.New(MathF.Min(2.5f * psionic.Amplification + psionic.Dampening, 15f)));
                solution.AddReagent("Epinephrine", FixedPoint2.New(MathF.Min(2.5f * psionic.Dampening + psionic.Amplification, 15f)));
                solution.AddReagent("Nocturine", 10f + (1 * psionic.Amplification + psionic.Dampening));
                _bloodstreamSystem.TryAddToChemicals(args.Target, solution, stream);
                _popupSystem.PopupEntity(Loc.GetString("regenerative-stasis-begin", ("entity", args.Target)), args.Target, PopupType.Medium);
                _psionics.LogPowerUsed(uid, "regenerative stasis", psionic, 4, 6);
                _actions.TryGetActionData(component.RegenerativeStasisActionEntity, out var actionData);
                if (actionData is { UseDelay: not null })
                    _actions.SetCooldown(component.RegenerativeStasisActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));
                args.Handled = true;
            }
        }
    }
}
