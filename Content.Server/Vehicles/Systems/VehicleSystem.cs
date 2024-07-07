using Content.Server.Vehicles.Components;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Vehicle;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
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
        [Dependency] private readonly SharedBuckleSystem _buckleSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<VehicleComponent, GetVerbsEvent<AlternativeVerb>>(AddEnterVehicleVerb);
            SubscribeLocalEvent<VehicleComponent, MoveInputEvent>(OnMoveInput);
            SubscribeLocalEvent<VehicleComponent, InteractHandEvent>(OnInteractHand);
            SubscribeLocalEvent<VehicleComponent, BuckleChangeEvent>(OnBuckleChange);
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
            if (component.Occupants.Count >= component.MaxOccupants)
            {
                _popupSystem.PopupEntity("The vehicle is full.", vehicle, Filter.Entities(user));
                return;
            }

            if (_buckleSystem.TryBuckle(user, user, vehicle))
            {
                component.Occupants.Add(user);
                if (component.Driver == null)
                {
                    component.Driver = user;
                }
            }
        }

        private void OnMoveInput(EntityUid uid, VehicleComponent component, ref MoveInputEvent args)
        {
            if (component.Driver == null || component.Driver != args.User)
                return;

            // Handle vehicle movement logic here.
            var transform = EntityManager.GetComponent<TransformComponent>(uid);
            var direction = args.Direction.ToVec();
            transform.Coordinates += direction * component.Speed * _gameTiming.FrameTime;
        }

        private void OnInteractHand(EntityUid uid, VehicleComponent component, InteractHandEvent args)
        {
            if (args.Handled)
                return;

            if (component.Occupants.Contains(args.User))
            {
                _buckleSystem.TryUnbuckle(args.User, args.User, false);
            }
            else
            {
                EnterVehicle(args.User, uid, component);
            }

            args.Handled = true;
        }

        private void OnBuckleChange(EntityUid uid, VehicleComponent component, BuckleChangeEvent args)
        {
            if (args.Buckled)
            {
                component.Occupants.Add(args.BuckleEntity);
            }
            else
            {
                component.Occupants.Remove(args.BuckleEntity);
                if (component.Driver == args.BuckleEntity)
                {
                    component.Driver = component.Occupants.Count > 0 ? component.Occupants[0] : null;
                }
            }
        }
    }
}
