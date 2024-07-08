using System.Numerics;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Vehicle;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.Vehicle
{
    public sealed class RiddenVehicleSystem : EntitySystem
    {
        [Dependency] private readonly SharedBuckleSystem _buckleSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RiddenVehicleComponent, InteractHandEvent>(OnInteractHand);
            SubscribeLocalEvent<RiddenVehicleComponent, Content.Shared.Buckle.Components.BuckleChangeEvent>(OnBuckleChange);
            SubscribeLocalEvent<RiddenVehicleComponent, MoveInputEvent>(OnMoveInput);
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

        private void OnBuckleChange(EntityUid uid, RiddenVehicleComponent component, Content.Shared.Buckle.Components.BuckleChangeEvent args)
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

        private void OnMoveInput(EntityUid uid, RiddenVehicleComponent component, MoveInputEvent args)
        {
            if (!component.Riders.Contains(args.Entity))
                return;

            var transform = _entityManager.GetComponent<TransformComponent>(uid);
            var worldPos = _transformSystem.GetWorldPosition(transform);
            var moveDir = ConvertMoveButtonsToVector(args.Component.HeldMoveButtons);
            var newPos = worldPos + (moveDir * component.Speed * (float) _gameTiming.FrameTime.TotalSeconds);

            _transformSystem.SetWorldPosition(transform, newPos);
        }

        private Vector2 ConvertMoveButtonsToVector(MoveButtons buttons)
        {
            var direction = Vector2.Zero;

            if (buttons.HasFlag(MoveButtons.Up))
                direction += Vector2.UnitY;
            if (buttons.HasFlag(MoveButtons.Down))
                direction -= Vector2.UnitY;
            if (buttons.HasFlag(MoveButtons.Left))
                direction -= Vector2.UnitX;
            if (buttons.HasFlag(MoveButtons.Right))
                direction += Vector2.UnitX;

            return direction.Normalized();
        }
    }
}
