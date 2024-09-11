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
using Content.Shared.DeltaV.Harpy.Events;

namespace Content.Shared.DeltaV.Harpy
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
        [Dependency] private readonly INetManager _net = default!;
        [Dependency] private readonly SharedGravitySystem _gravity = default!;
        [Dependency] private readonly SharedBodySystem _bodySystem = default!;
        private float _frictionModifier;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DashComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<DashComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<DashComponent, DashDoAfterEvent>(OnDashDoAfter);
            SubscribeLocalEvent<DashComponent, ActivateDashEvent>(OnDashAttempt);
            SubscribeLocalEvent<DashComponent, PhysicsSleepEvent>(OnSleep);
            SubscribeLocalEvent<DashComponent, StartCollideEvent>(HandleCollision);
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

                var stopDashTime = (dash.DashLandTime ?? dash.DashStartTime) + TimeSpan.FromSeconds(dash.GlideDuration);

                if (stopDashTime <= _timing.CurTime && dash.Dashing)
                {
                    _physics.SetBodyStatus(uid, physics, BodyStatus.OnGround);
                }
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

        private void OnDashAttempt(EntityUid uid, DashComponent component, ActivateDashEvent args)
        {
            if (CanDash(uid))
            {
                // EntityCoordinates cannot be passed as a serializable argument so it'll stay on the component.
                component.TargetCoordinates = args.Target;
                var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.ActivationDelay, new DashDoAfterEvent(), uid, target: uid)
                {
                    BlockDuplicate = true,
                    BreakOnUserMove = true,
                    BreakOnDamage = true,
                    NeedHand = true
                };

                if (!_doAfter.TryStartDoAfter(doAfterArgs))
                {
                    return;
                }
            }
        }

        private void OnDashDoAfter(EntityUid uid, DashComponent component, DashDoAfterEvent args)
        {
            if (args.Handled || args.Cancelled)
                return;

            StartDash(uid, component);
            _actionsSystem.TryGetActionData(component.DashActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actionsSystem.StartUseDelay(component.DashActionEntity);
            args.Handled = true;
        }

        private void StartDash(EntityUid uid, DashComponent component)
        {
            if (!TryComp<PhysicsComponent>(uid, out var physics))
                return;

            var tileFriction = _frictionModifier * TileFrictionController.DefaultFriction;
            var startPos = _transform.GetMapCoordinates(uid).Position;
            var targetPos = component.TargetCoordinates.ToMap(EntityManager, _transform).Position;
            var diff = targetPos - startPos;

            var flyTime = diff.Length() / component.DashSpeed * component.LungeDuration;

            if (diff == Vector2Helpers.Infinity || diff == Vector2Helpers.NaN || diff == Vector2.Zero || tileFriction < 0)
                return;

            component.Landed = false;
            component.DashStartTime = _timing.CurTime;
            component.DashLandTime = component.DashStartTime + TimeSpan.FromSeconds(flyTime);

            // Does the user take damage when colliding with hard fixtures?
            if (component.CollisionProperties.Bonkable && component.DamagingProperties == null)
                _impact.ChangeCollide(uid, component.CollisionProperties.MinimumSpeed, component.CollisionProperties.StunSeconds, component.CollisionProperties.DamageCooldown, component.CollisionProperties.SpeedDamage);

            var dashSpeed = (tileFriction > 0f) ? diff.Length() / (flyTime + 1.5f / tileFriction) : component.DashSpeed;
            var impulseVector = diff.Normalized() * dashSpeed * physics.Mass;
            FreeHands(uid);

            // The slowdown after the jump is enough of a punishment, letting them stamcrit midflight would be silly.
            _stamina.TryTakeStamina(uid, component.StaminaDrain * diff.Length());
            _physics.ApplyLinearImpulse(uid, impulseVector, body: physics);
            _physics.SetBodyStatus(uid, physics, BodyStatus.InAir);
            component.Dashing = true;
        }

        private void FreeHands(EntityUid uid)
        {
            if (!TryComp<HandsComponent>(uid, out var handsComponent))
                return;

            foreach (var hand in _hands.EnumerateHands(uid, handsComponent))
            {
                // Is this entity removable? (they might have handcuffs on)
                if (HasComp<UnremoveableComponent>(hand.HeldEntity) && hand.HeldEntity != uid)
                    continue;

                _hands.DoDrop(uid, hand, true, handsComponent);
            }
        }

        private bool CanDash(EntityUid uid)
        {
            var cuffed = TryComp<CuffableComponent>(uid, out var cuffableComp) && !cuffableComp.CanStillInteract;
            var zombified = TryComp<ZombieComponent>(uid, out var _);

            // Tell the user that they can not dash.
            if (cuffed || zombified)
            {
                //Todo, make a new string for this
                _popupSystem.PopupEntity(Loc.GetString("no-flight-while-restrained"), uid, uid, PopupType.Medium);
                return false;
            }
            return true;
        }

        private void HandleCollision(EntityUid uid, DashComponent component, ref StartCollideEvent args)
        {
            if (!component.Landed)
            {
                CollideInteraction(component, args.OurEntity, args.OtherEntity, args.OtherFixture.Hard);
            }
        }

        private void OnSleep(EntityUid uid, DashComponent component, ref PhysicsSleepEvent @event)
        {
            if (component.Dashing)
            {
                StopDash(uid, component);
            }
        }

        public void StopDash(EntityUid uid, DashComponent component)
        {
            if (!TryComp<PhysicsComponent>(uid, out var physics))
                return;

            _physics.SetBodyStatus(uid, physics, BodyStatus.OnGround);

            component.Dashing = false;
        }

        public void LandDash(EntityUid uid, DashComponent component, PhysicsComponent physics)
        {
            if (component.Landed || _gravity.IsWeightless(uid) || Deleted(uid))
                return;

            // We reset collision on impact values to the defaults if the user can collide. These are hardcoded since theres no CVARs yet.
            if (component.CollisionProperties.Bonkable)
                _impact.ChangeCollide(uid, 20f, 1f, 2f, 0.5f);

            component.Landed = true;
            //TODO: Play audio here
        }

        public void CollideInteraction(DashComponent component, EntityUid dashingEntity, EntityUid target, bool isHardFixture)
        {
            if (component.DamagingProperties != null)
            {
                    if (TryComp<BodyComponent>(target, out var body))
                {
                    Logger.Debug($"Colliding with entity {target} with {body}, with {component.DamagingProperties.Gibber}");
                    if (component.DamagingProperties.Gibber)
                    {
                        Logger.Debug($"Attempting to gib entity {target}");
                        _bodySystem.GibBody(target, gibOrgans: true, body: body);
                        return;
                    }
                }
                else if (component.DamagingProperties.DestroyTiles && isHardFixture)
                {
                    Logger.Debug($"Attempting to delete entity!");
                    QueueDel(target);
                }
            }
            /* Todo: Define events from collisions that can be used for other stuffs,
            I.E. destroy tiles when the entity clashes into them, deal damage to anything that passes, etc. */
        }

    }
    public sealed partial class ActivateDashEvent : WorldTargetActionEvent
    {
    }
}