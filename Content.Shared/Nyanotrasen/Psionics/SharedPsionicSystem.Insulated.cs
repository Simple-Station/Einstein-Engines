using Content.Shared.Abilities.Psionics;

namespace Content.Shared.Psionics
{
    public sealed class PsionicInsulationSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicInsulationComponent, ComponentInit>(OnInsulated);
        }

        public void OnInsulated(EntityUid uid, PsionicInsulationComponent component, ComponentInit args)
        {
            RaiseLocalEvent(uid, new PsionicInsulationEvent());
        }
    }
    public readonly record struct PsionicInsulationEvent;
}
