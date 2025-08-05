using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Server.Stunnable;
using Content.Server.Beam;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicAssaultPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly BeamSystem _beam = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicAssaultPowerActionEvent>(OnPowerUsed);
        }

        private void OnPowerUsed(PsionicAssaultPowerActionEvent args)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, args.Target, "Psionic Assault", true))
                return;

            _beam.TryCreateBeam(args.Performer, args.Target, "LightningNoospheric");

            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(5), false);
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Stutter", TimeSpan.FromSeconds(10), false, "StutteringAccent");

            _psionics.LogPowerUsed(args.Performer, "Psionic Assault");
            args.Handled = true;
        }
    }
}
