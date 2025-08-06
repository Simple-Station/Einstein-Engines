using Content.Server.Beam;
using Content.Server.Medical;
using Content.Server.Stunnable;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.StatusEffect;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicAssaultPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly BeamSystem _beam = default!;
        [Dependency] private readonly VomitSystem _vomitSystem = default!;

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

            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(10), false);
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Stutter", TimeSpan.FromSeconds(30), false, "StutteringAccent");
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "PsionicsDisabled", TimeSpan.FromSeconds(100), false, "PsionicsDisabled");
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "PsionicallyInsulated", TimeSpan.FromSeconds(100), false, "PsionicInsulation");
            _vomitSystem.Vomit(args.Target, -120, -120);
            _psionics.LogPowerUsed(args.Performer, "Psionic Assault");
            args.Handled = true;
        }
    }
}
