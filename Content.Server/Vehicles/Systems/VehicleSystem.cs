// /Content.Server/Vehicles/Systems/VehicleSystem.cs
using Content.Server.Vehicles.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Vehicles.Systems
{
    public sealed class VehicleSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<VehicleComponent, GetVerbsEvent<AlternativeVerb>>(AddEnterVehicleVerb);
            SubscribeLocalEvent<VehicleComponent, MoveInputEvent>(OnMoveInput);
        }

        private void AddEnterVehicleVerb(EntityUid uid, VehicleComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (component.Occupants.Count >= component.MaxOccupants)
                return;

            AlternativeVerb verb = new()
            {
                Act = () => EnterVehicle(args.User, uid, component),
                Text = Loc.GetString("enter-vehicle-verb"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void EnterVehicle(EntityUid user, EntityUid vehicle, VehicleComponent component)
        {
            component.Occupants.Add(user);
            if (component.Driver == null)
            {
                component.Driver = user;
            }
            //TODO  Additional logic to attach user to vehicle, update UI, etc maybe.
        }

        private void OnMoveInput(EntityUid uid, VehicleComponent component, ref MoveInputEvent args)
        {
            if (component.Driver == null || component.Driver != args.User)
                return;

            // Handle vehicle movement logic here. Surely theres a component for that im just tossing ideas
        }
    }
}
