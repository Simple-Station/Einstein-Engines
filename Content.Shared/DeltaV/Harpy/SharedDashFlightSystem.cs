using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using System.Numerics;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Popups;
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
    public class SharedDashFlightSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly StaminaSystem _stamina = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly INetManager _net = default!;
        [Dependency] private readonly ThrownItemSystem _thrownSystem = default!;
        private float _dashDistance;
        private float _frictionModifier;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DashFlightComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<DashFlightComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<DashFlightComponent, DashDoAfterEvent>(OnDashDoAfter);
            SubscribeLocalEvent<DashFlightComponent, DashFlightEvent>(OnDashAttempt);
            Subs.CVar(_configManager, CCVar.CCVars.TileFrictionModifier, value => _frictionModifier = value, true);
        }

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
            if (CanFly(uid))
            {
                // EntityCoordinates cannot be passed as a serializable argument so it'll stay on the system.
                component.TargetCoordinates = args.Target;
                var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.ActivationDelay, new DashDoAfterEvent(), uid, target: uid)
                {
                    BlockDuplicate = true,
                    BreakOnTargetMove = true,
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

        private void OnDashDoAfter(EntityUid uid, DashFlightComponent component, DashDoAfterEvent args)
        {
            if (args.Handled || args.Cancelled)
                return;

            StartDash(uid, component);
            _actionsSystem.TryGetActionData(component.DashActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actionsSystem.StartUseDelay(component.DashActionEntity);
            args.Handled = true;
        }

        private void StartDash(EntityUid uid, DashFlightComponent component)
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

            // TODO: Figure out how to make a component that will take any given visualizer, apply it here.
            var comp = new ThrownItemComponent
            {
                Thrower = uid,
                Animate = true,
            };
            comp.ThrownTime = _timing.CurTime;
            comp.LandTime = comp.ThrownTime + TimeSpan.FromSeconds(flyTime);
            comp.PlayLandSound = true;
            AddComp(uid, comp, true);

            var dashSpeed = (tileFriction > 0f) ? diff.Length() / (flyTime + 1.5f / tileFriction) : component.DashSpeed;
            var impulseVector = diff.Normalized() * dashSpeed * physics.Mass;
            FreeHands(uid);
            // The slowdown after the jump is enough of a punishment, letting them stamcrit midflight would be silly.
            _stamina.TryTakeStamina(uid, component.StaminaDrain * diff.Length());
            _physics.ApplyLinearImpulse(uid, impulseVector, body: physics);
            _physics.SetBodyStatus(uid, physics, BodyStatus.InAir);
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

        private bool CanFly(EntityUid uid)
        {
            var cuffed = TryComp<CuffableComponent>(uid, out var cuffableComp) && !cuffableComp.CanStillInteract;
            var zombified = TryComp<ZombieComponent>(uid, out var _);

            // Tell the user that they can not fly.
            if (cuffed || zombified)
            {
                _popupSystem.PopupEntity(Loc.GetString("no-flight-while-restrained"), uid, uid, PopupType.Medium);
                return false;
            }
            return true;
        }

    }
    public sealed partial class DashFlightEvent : WorldTargetActionEvent
    {
    }
}