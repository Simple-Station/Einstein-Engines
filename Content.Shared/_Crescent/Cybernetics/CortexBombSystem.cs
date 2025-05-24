using Content.Shared.Actions;

namespace Content.Shared.Cybernetics
{
    public sealed class CortexBombSystem : EntitySystem
    {

        [Dependency] private readonly SharedActionsSystem _actions = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CortexBombComponent, CortexBombActivatedEvent>(OnCortexBombActivated);

            SubscribeLocalEvent<CortexBombComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<CortexBombComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnCortexBombActivated(EntityUid uid, CortexBombComponent component, CortexBombActivatedEvent args)
        {
            throw new NotImplementedException("i'm lazy");
        }

        private void OnStartup(EntityUid uid, CortexBombComponent component, ComponentStartup args)
        {
            component.Action = _actions.AddAction(uid, component.ActionPrototype);
        }

        private void OnShutdown(EntityUid uid, CortexBombComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.ActionPrototype);
        }
    }
}
