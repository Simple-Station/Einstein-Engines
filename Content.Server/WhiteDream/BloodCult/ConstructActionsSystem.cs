using Content.Shared.StatusEffect;
using Content.Shared.WhiteDream.BloodCult.Constructs.PhaseShift;
using Content.Shared.WhiteDream.BloodCult.Spells;

namespace Content.Server.WhiteDream.BloodCult;

public sealed class ConstructActionsSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PhaseShiftEvent>(OnPhaseShift);
    }

    private void OnPhaseShift(PhaseShiftEvent args)
    {
        if (args.Handled)
            return;

        if (_statusEffects.TryAddStatusEffect<PhaseShiftedComponent>(
            args.Performer,
            args.StatusEffectId,
            args.Duration,
            false))
            args.Handled = true;
    }
}
