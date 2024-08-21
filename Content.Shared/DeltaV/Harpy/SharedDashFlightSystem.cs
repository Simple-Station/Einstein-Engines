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
            var query = EntityQueryEnumerator<DashFlightComponent, PhysicsComponent>();
            while (query.MoveNext(out var uid, out var dash, out var physics))
            {
                if (!dash.IsDashing)
                {
                    continue;
                }

                var movement = dash.DashDirection * dash.DashSpeed * frameTime;
                var movementDistance = movement.Length();

                if (movementDistance > dash.RemainingDistance)
                {
                    movement = dash.DashDirection * dash.RemainingDistance;
                    dash.IsDashing = false;
                }

                dash.RemainingDistance -= movementDistance;

                _physics.SetLinearVelocity(uid, movement / frameTime, body: physics);

                if (!dash.IsDashing)
                {
                    // Dash completed
                    _physics.SetLinearVelocity(uid, Vector2.Zero, body: physics);
                    // TODO: Play dash sound here
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
            if (_net.IsServer)
            {
                var startPos = _transform.GetMapCoordinates(uid).Position;
                var targetPos = args.Target.ToMap(EntityManager, _transform).Position;
                var diff = targetPos - startPos;
                component.TargetCoordinates = targetPos;
                component.DashDirection = diff.Normalized();
                component.RemainingDistance = Math.Clamp(diff.Length(), component.MinDashDistance, component.MaxDashDistance);
                component.IsDashing = true;
            }

            //Logger.Debug($"Starting position {startPos}, target {targetPos}, direction {DashDirection}, frametime {_timing.FrameTime}");
            //var velocity = diff * component.DashSpeed;
            //Logger.Debug($"Distance {totalDistance}, Clamped Distance {clampedDistance}, DashTime {dashTime}, Velocity {velocity}");
            //_physics.ApplyLinearImpulse(uid, velocity);

            // Set up a timer to stop the dash
            //Timer.Spawn(TimeSpan.FromSeconds(dashTime), () => StopDash(uid));

            //component.LastDashTime = _timing.CurTime;
            //args.Handled = true;
        }

        /*     private bool CanDash(EntityUid uid, DashComponent component)
             {
                 if (GameTiming.CurTime - component.LastDashTime < TimeSpan.FromSeconds(component.Cooldown))
                 {
                     _popup.PopupEntity($"Dash is on cooldown for {component.Cooldown - (GameTiming.CurTime - component.LastDashTime).TotalSeconds:F1} seconds.", uid, uid);
                     return false;
                 }

                 return true;
             }*/


        #endregion

    }
    public sealed partial class DashFlightEvent : WorldTargetActionEvent
    {
    }
}