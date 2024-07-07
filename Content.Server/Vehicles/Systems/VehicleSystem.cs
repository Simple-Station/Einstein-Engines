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
            SubscribeLocalEvent<RiddenVehicleComponent, InteractHandEvent>(OnInteractHand);
            SubscribeLocalEvent<RiddenVehicleComponent, BuckleChangeEvent>(OnBuckleChange);
        }

        private void OnInteractHand(EntityUid uid, RiddenVehicleComponent component, InteractHandEvent args)
        {
            if (args.Handled)
                return;

            if (component.Riders.Contains(args.User))
            {
                _buckleSystem.TryUnbuckle(args.User, args.User, false);
            }
            else
            {
                _buckleSystem.TryBuckle(args.User, args.User, uid);
            }

            args.Handled = true;
        }

        private void OnBuckleChange(EntityUid uid, RiddenVehicleComponent component, BuckleChangeEvent args)
        {
            if (args.Buckling)
            {
                component.Riders.Add(args.BuckledEntity);
            }
            else
            {
                component.Riders.Remove(args.BuckledEntity);
            }
        }
    }
}
