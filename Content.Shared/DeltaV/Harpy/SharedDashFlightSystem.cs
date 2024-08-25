using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Shared.DeltaV.Harpy
{
    public class SharedDashFlightSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DashFlightComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<DashFlightComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<DashFlightComponent, DashFlightEvent>(OnDashAttempt);
        }

        public override void Update(float frameTime)
        {
            const float stopThreshold = 0.05f;
            var query = EntityQueryEnumerator<DashFlightComponent, PhysicsComponent>();
            while (query.MoveNext(out var uid, out var dash, out var physics))
            {
                if (_timing.IsFirstTimePredicted && dash.IsDashing)
                {
                    var progress = 1 - dash.RemainingDistance / dash.MaxDashDistance;
                    var currentSpeed = MathHelper.Lerp(dash.DashSpeed, 0, progress);

                    var movement = dash.DashDirection * currentSpeed * frameTime;
                    var movementDistance = movement.Length();

                    Logger.Debug($"calculated movement {movement}, current distance {movementDistance}, remaining distance {dash.RemainingDistance}, current speed {currentSpeed}, current progress {progress}");
                    if (movementDistance < stopThreshold)
                    {
                        dash.IsDashing = false;
                    }
                    _physics.SetLinearVelocity(uid, movement / frameTime, body: physics);
                    dash.RemainingDistance -= movementDistance;
                }

            }
        }

        #region Core Functions
        private void OnStartup(EntityUid uid, DashFlightComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, ref component.DashActionEntity, component.DashAction);
        }

        private void OnShutdown(EntityUid uid, DashFlightComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.DashActionEntity);
        }

        private void OnDashAttempt(EntityUid uid, DashFlightComponent component, DashFlightEvent args)
        {
            if (_timing.IsFirstTimePredicted && _net.IsServer)
            {
                Logger.Debug("Executing dashattempt");
                var startPos = _transform.GetMapCoordinates(uid).Position;
                var targetPos = args.Target.ToMap(EntityManager, _transform).Position;
                var diff = targetPos - startPos;
                component.TargetCoordinates = targetPos;
                component.DashDirection = diff.Normalized();
                component.RemainingDistance = Math.Clamp(diff.Length(), component.MinDashDistance, component.MaxDashDistance);
                component.IsDashing = true;
            }
        }

        #endregion

    }
    public sealed partial class DashFlightEvent : WorldTargetActionEvent
    {
    }
}