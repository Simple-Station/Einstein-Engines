using Content.Shared.Actions;
using Content.Shared.Psionics.Abilities;
using Content.Shared.StatusEffect;
using Content.Server.Electrocution;
using Content.Server.Stunnable;
using Content.Server.Beam;
using Content.Shared.Actions.Events;

namespace Content.Server.Psionics.Abilities
{
    public sealed class NoosphericZapPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly BeamSystem _beam = default!;
        [Dependency] private readonly ElectrocutionSystem _electrocution = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<NoosphericZapPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<NoosphericZapPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<NoosphericZapPowerComponent, NoosphericZapPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, NoosphericZapPowerComponent component, ComponentInit args)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);
            _actions.AddAction(uid, ref component.NoosphericZapActionEntity, component.NoosphericZapActionId );
            _actions.TryGetActionData(component.NoosphericZapActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.SetCooldown(component.NoosphericZapActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));

            psionic.ActivePowers.Add(component);
            psionic.PsychicFeedback.Add(component.NoosphericZapFeedback);
            psionic.Amplification += 1f;
        }

        private void OnShutdown(EntityUid uid, NoosphericZapPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.NoosphericZapActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.NoosphericZapFeedback);
                psionic.Amplification -= 1f;
            }
        }

        private void OnPowerUsed(EntityUid uid, NoosphericZapPowerComponent component, NoosphericZapPowerActionEvent args)
        {
            if (_psionics.CheckCanTargetCast(uid, args.Target, out var psionic))
            {

                _actions.TryGetActionData(component.NoosphericZapActionEntity, out var actionData);
                if (actionData is { UseDelay: not null })
                    _actions.SetCooldown(component.NoosphericZapActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));
                _beam.TryCreateBeam(args.Performer, args.Target, "LightningNoospheric");
                _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(1 * psionic.Amplification), false);

                _electrocution.TryDoElectrocution(args.Target, null,
                    (int) MathF.Round(5f * psionic.Amplification),
                    new TimeSpan((long) MathF.Round(1f * psionic.Amplification)),
                    true,
                    ignoreInsulation: true);

                _statusEffectsSystem.TryAddStatusEffect(args.Target, "Stutter", TimeSpan.FromSeconds(2 * psionic.Amplification), false, "StutteringAccent");

                _psionics.LogPowerUsed(args.Performer, "noopsheric zap", psionic, 6, 8);
                args.Handled = true;
            }
        }
    }
}
