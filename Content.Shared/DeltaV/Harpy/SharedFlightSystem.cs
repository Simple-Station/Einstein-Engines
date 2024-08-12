
using Content.Shared.Actions;
using Content.Shared.Movement.Components;
using Content.Shared.Gravity;


namespace Content.Shared.DeltaV.Harpy
{
    public class SharedFlightSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<FlightComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<FlightComponent, ComponentShutdown>(OnShutdown);
            //SubscribeLocalEvent<FlightComponent, ToggleFlightEvent>(OnToggleFlight);
        }

        private void OnStartup(EntityUid uid, FlightComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
        }

        private void OnShutdown(EntityUid uid, FlightComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.ToggleActionEntity);
        }

        public void ToggleActive(bool active, FlightComponent component)
        {
            component.On = active;
            _actionsSystem.SetToggled(component.ToggleActionEntity, component.On);
        }
    }
    public sealed partial class ToggleFlightEvent : InstantActionEvent {}
}
