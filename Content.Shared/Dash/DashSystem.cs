using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Content.Shared.Gravity;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using System.Numerics;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Movement.Components;
using Content.Shared.CCVar;
using Content.Shared.Friction;
using Robust.Shared.Configuration;
using Content.Shared.Interaction.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared.Dash
{
    public sealed class SharedDashSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly DamageOnHighSpeedImpactSystem _impact = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly StaminaSystem _stamina = default!;
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly SharedGravitySystem _gravity = default!;
        [Dependency] private readonly INetManager _net = default!;

        private float _frictionModifier;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DashComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<DashComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<DashComponent, ActivateDashEvent>(OnDashAction);
            SubscribeLocalEvent<DashComponent, PhysicsSleepEvent>(OnSleep);
            Subs.CVar(_configManager, CCVar.CCVars.TileFrictionModifier, value => _frictionModifier = value, true);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<DashComponent, PhysicsComponent>();
            while (query.MoveNext(out var uid, out var dash, out var physics))
            {
                if (dash.DashLandTime <= _timing.CurTime && !dash.Landed)
                    LandDash(uid, dash, physics);

                if (dash.Dashing && dash.DashLandTime <= _timing.CurTime)
                    StopDash(uid, dash, physics);
            }
        }

        private void OnStartup(EntityUid uid, DashComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, ref component.DashActionEntity, component.DashAction);
        }

        private void OnShutdown(EntityUid uid, DashComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.DashActionEntity);
        }

        private void OnDashAction(EntityUid uid, DashComponent component, ActivateDashEvent args)
        {
            if (args.Handled)
                return;

            StartDash(uid, component, args.Target);
            args.Handled = true;
        }

        private void StartDash(EntityUid uid, DashComponent component, EntityCoordinates target)
        {
            if (!CanDash(uid) ||
                !TryComp<PhysicsComponent>(uid, out var physics) ||
                !TryComp<MovementSpeedModifierComponent>(uid, out var movement))
                return;

            if (TryComp<StaminaComponent>(uid, out var stamina)
                && stamina.CritThreshold - stamina.StaminaDamage <= component.StaminaCost)
            {
                _popupSystem.PopupClient(Loc.GetString("dash-no-stamina"), uid, uid, PopupType.Medium);
                return;
            }

            if (_gravity.IsWeightless(uid, physics))
            {
                _popupSystem.PopupClient(Loc.GetString("dash-weightless"), uid, uid, PopupType.Medium);
                return;
            }

            _actionsSystem.TryGetActionData(component.DashActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actionsSystem.StartUseDelay(component.DashActionEntity);

            var tileFriction = _frictionModifier * TileFrictionController.DefaultFriction;
            var startPos = _transform.GetMapCoordinates(uid).Position;
            var targetPos = target.ToMap(EntityManager, _transform).Position;

            var diff = targetPos - startPos;

            var unitVector = Vector2.Normalize(diff);

            if (unitVector == Vector2Helpers.Infinity || unitVector == Vector2Helpers.NaN || unitVector == Vector2.Zero || tileFriction < 0)
                return;

            component.Landed = false;
            component.DashStartTime = _timing.CurTime;
            component.DashLandTime = component.DashStartTime + TimeSpan.FromSeconds(component.DashDuration);

            var dashSpeed = movement.CurrentSprintSpeed * component.DashSpeedMultiplier;
            var impulseVector = unitVector * dashSpeed * physics.Mass;

            _physics.SetBodyStatus(uid, physics, BodyStatus.InAir);

            var invertedImpulseVector = impulseVector * physics.InvMass;
            var linearImpulseVelocity = physics.LinearVelocity + invertedImpulseVector;
            var maxSpeed = invertedImpulseVector.Length();

            // Check to prevent the dash from going too fast
            if (linearImpulseVelocity.Length() > maxSpeed)
            {
                // Redirect already fast momentum
                if (physics.LinearVelocity.Length() > invertedImpulseVector.Length())
                {
                    _physics.SetLinearVelocity(uid, impulseVector.Normalized() * physics.LinearVelocity.Length(), body: physics);
                }
                // Speed up to max possible velocity (same speed as if dashing from standing still)
                else
                {
                    _physics.SetLinearVelocity(uid, invertedImpulseVector, body: physics);
                }
            }
            // Standing still or dashing in the opposite direction of current velocity
            else
            {
                _physics.SetLinearVelocity(uid, linearImpulseVelocity, body: physics);
            }

            component.Dashing = true;

            _stamina.TakeStaminaDamage(uid, component.StaminaCost, stamina, visual: false, causesDecayCooldown: false);
        }

        private bool CanDash(EntityUid uid)
        {
            var cuffed = TryComp<CuffableComponent>(uid, out var cuffableComp) && !cuffableComp.CanStillInteract;
            var zombified = TryComp<ZombieComponent>(uid, out var _);

            if (cuffed || zombified)
            {
                _popupSystem.PopupEntity(Loc.GetString("dash-disabled"), uid, uid, PopupType.Medium);
                return false;
            }
            return true;
        }

        private void OnSleep(EntityUid uid, DashComponent component, ref PhysicsSleepEvent @event)
        {
            if (!component.Dashing)
                return;

            StopDash(uid, component);
        }

        private void StopDash(EntityUid uid, DashComponent component, PhysicsComponent? physics = null)
        {
            if (!Resolve(uid, ref physics))
                return;

            component.Dashing = false;
            _physics.SetBodyStatus(uid, physics, BodyStatus.OnGround, dirty: false);
        }

        private void LandDash(EntityUid uid, DashComponent component, PhysicsComponent physics)
        {
            if (component.Landed || _gravity.IsWeightless(uid) || Deleted(uid))
                return;

            component.Landed = true;

            //TODO: Play audio here
        }
    }

    public sealed partial class ActivateDashEvent : WorldTargetActionEvent
    {
    }
}
