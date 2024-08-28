using System.Numerics;
using Content.Shared._White.Grab;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Buckle.Components;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Pulling.Events;
using Content.Shared.Speech;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Movement.Pulling.Systems;

/// <summary>
/// Allows one entity to pull another behind them via a physics distance joint.
/// </summary>
public sealed class PullingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifierSystem = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _xformSys = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SharedPhysicsSystem));
        UpdatesOutsidePrediction = true;

        SubscribeLocalEvent<PullableComponent, MoveInputEvent>(OnPullableMoveInput);
        SubscribeLocalEvent<PullableComponent, CollisionChangeEvent>(OnPullableCollisionChange);
        SubscribeLocalEvent<PullableComponent, JointRemovedEvent>(OnJointRemoved);
        SubscribeLocalEvent<PullableComponent, GetVerbsEvent<Verb>>(AddPullVerbs);
        SubscribeLocalEvent<PullableComponent, EntGotInsertedIntoContainerMessage>(OnPullableContainerInsert);
        SubscribeLocalEvent<PullableComponent, UpdateCanMoveEvent>(OnGrabbedMoveAttempt);
        SubscribeLocalEvent<PullableComponent, SpeakAttemptEvent>(OnGrabbedSpeakAttempt);

        SubscribeLocalEvent<PullerComponent, EntGotInsertedIntoContainerMessage>(OnPullerContainerInsert);
        SubscribeLocalEvent<PullerComponent, EntityUnpausedEvent>(OnPullerUnpaused);
        SubscribeLocalEvent<PullerComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
        SubscribeLocalEvent<PullerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<PullerComponent, VirtualItemThrownEvent>(OnVirtualItemThrown);
        SubscribeLocalEvent<PullerComponent, VirtualItemDropAttemptEvent>(OnVirtualItemDropAttempt);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.MovePulledObject, new PointerInputCmdHandler(OnRequestMovePulledObject))
            .Bind(ContentKeyFunctions.ReleasePulledObject, InputCmdHandler.FromDelegate(OnReleasePulledObject, handle: false))
            .Register<PullingSystem>();
    }

    private void OnPullerContainerInsert(Entity<PullerComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (ent.Comp.Pulling == null) return;

        if (!TryComp(ent.Comp.Pulling.Value, out PullableComponent? pulling))
            return;

        foreach (var item in ent.Comp.GrabVirtualItems)
        {
            QueueDel(item);
        }

        TryStopPull(ent.Comp.Pulling.Value, pulling, ent.Owner, true);
    }

    private void OnPullableContainerInsert(Entity<PullableComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        TryStopPull(ent.Owner, ent.Comp, ignoreGrab: true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<PullingSystem>();
    }

    private void OnPullerUnpaused(EntityUid uid, PullerComponent component, ref EntityUnpausedEvent args)
    {
        component.NextThrow += args.PausedTime;
    }

    private void OnVirtualItemDropAttempt(EntityUid uid, PullerComponent component, VirtualItemDropAttemptEvent args)
    {
        if (component.Pulling == null)
            return;

        if (component.Pulling != args.BlockingEntity)
            return;

        if (_timing.CurTime < component.NextStageChange)
        {
            args.Cancel();  // VirtualItem is NOT being deleted
            return;
        }

        if (!args.Throw)
        {
            if (component.GrabStage > GrabStage.No)
            {
                if (EntityManager.TryGetComponent(args.BlockingEntity, out PullableComponent? comp))
                {
                    TryLowerGrabStage(component.Pulling.Value, uid);
                    args.Cancel();  // VirtualItem is NOT being deleted
                }
            }
        }
        else
        {
            if (component.GrabStage <= GrabStage.Soft)
            {
                TryLowerGrabStage(component.Pulling.Value, uid);
                args.Cancel();  // VirtualItem is NOT being deleted
            }
        }
    }

    private void OnVirtualItemDeleted(EntityUid uid, PullerComponent component, VirtualItemDeletedEvent args)
    {
        // If client deletes the virtual hand then stop the pull.
        if (component.Pulling == null)
            return;

        if (component.Pulling != args.BlockingEntity)
            return;

        if (EntityManager.TryGetComponent(args.BlockingEntity, out PullableComponent? comp))
        {
            TryLowerGrabStage(component.Pulling.Value, uid);
        }
    }

    private void OnVirtualItemThrown(EntityUid uid, PullerComponent component, VirtualItemThrownEvent args)
    {
        if (component.Pulling == null)
            return;

        if (component.Pulling != args.BlockingEntity)
            return;

        if (EntityManager.TryGetComponent(args.BlockingEntity, out PullableComponent? comp))
        {
            if (_combatMode.IsInCombatMode(uid) &&
                !HasComp<GrabThrownComponent>(args.BlockingEntity) &&
                component.GrabStage > GrabStage.Soft)
            {
                var direction = args.Direction;
                var vecBetween = (Transform(args.BlockingEntity).Coordinates.ToMapPos(EntityManager, _transform) - Transform(uid).WorldPosition);

                // Getting angle between us
                var dirAngle = direction.ToWorldAngle().Degrees;
                var betweenAngle = vecBetween.ToWorldAngle().Degrees;

                var angle = dirAngle - betweenAngle;

                if (angle < 0)
                    angle = -angle;

                var maxDistance = 3f;
                var damageModifier = 1f;

                if (angle < 30)
                {
                    damageModifier = 0.3f;
                    maxDistance = 1f;
                }
                else if (angle < 90)
                {
                    damageModifier = 0.7f;
                    maxDistance = 1.5f;
                }
                else
                    maxDistance = 2.25f;

                var distance = Math.Clamp(args.Direction.Length(), 0.5f, maxDistance);
                direction *= distance / args.Direction.Length();


                var damage = new DamageSpecifier();
                damage.DamageDict.Add("Blunt", 5);
                damage *= damageModifier;

                TryStopPull(args.BlockingEntity, comp, uid, true);
                _grabThrown.Throw(args.BlockingEntity, uid, direction * 2f, 120f, damage * component.GrabThrowDamageModifier, damage * component.GrabThrowDamageModifier);
                _throwing.TryThrow(uid, -direction * 0.5f);
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), uid);
                component.NextStageChange.Add(TimeSpan.FromSeconds(2f));  // To avoid grab and throw spamming
            }
        }
    }

    private void AddPullVerbs(EntityUid uid, PullableComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Are they trying to pull themselves up by their bootstraps?
        if (args.User == args.Target)
            return;

        //TODO VERB ICONS add pulling icon
        if (component.Puller == args.User)
        {
            Verb verb = new()
            {
                Text = Loc.GetString("pulling-verb-get-data-text-stop-pulling"),
                Act = () => TryStopPull(uid, component, user: args.User),
                DoContactInteraction = false // pulling handle its own contact interaction.
            };
            args.Verbs.Add(verb);
        }
        else if (CanPull(args.User, args.Target))
        {
            Verb verb = new()
            {
                Text = Loc.GetString("pulling-verb-get-data-text"),
                Act = () => TryStartPull(args.User, args.Target),
                DoContactInteraction = false // pulling handle its own contact interaction.
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnRefreshMovespeed(EntityUid uid, PullerComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (TryComp<HeldSpeedModifierComponent>(component.Pulling, out var heldMoveSpeed) && component.Pulling.HasValue)
        {
            var (walkMod, sprintMod) = (args.WalkSpeedModifier, args.SprintSpeedModifier);

            switch (component.GrabStage)
            {
                case GrabStage.No:
                    args.ModifySpeed(walkMod, sprintMod);
                    break;
                case GrabStage.Soft:
                    args.ModifySpeed(walkMod * 0.9f, sprintMod * 0.9f);
                    break;
                case GrabStage.Hard:
                    args.ModifySpeed(walkMod * 0.7f, sprintMod * 0.7f);
                    break;
                case GrabStage.Suffocate:
                    args.ModifySpeed(walkMod * 0.4f, sprintMod * 0.4f);
                    break;
                default:
                    args.ModifySpeed(walkMod, sprintMod);
                    break;
            }
            return;
        }

        switch (component.GrabStage)
        {
            case GrabStage.No:
                args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
                break;
            case GrabStage.Soft:
                args.ModifySpeed(component.WalkSpeedModifier * 0.9f, component.SprintSpeedModifier * 0.9f);
                break;
            case GrabStage.Hard:
                args.ModifySpeed(component.WalkSpeedModifier * 0.7f, component.SprintSpeedModifier * 0.7f);
                break;
            case GrabStage.Suffocate:
                args.ModifySpeed(component.WalkSpeedModifier * 0.4f, component.SprintSpeedModifier * 0.4f);
                break;
            default:
                args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
                break;
        }
    }

    private void OnPullableMoveInput(EntityUid uid, PullableComponent component, ref MoveInputEvent args)
    {
        // If someone moves then break their pulling.
        if (!component.BeingPulled)
            return;

        var entity = args.Entity;

        if (!_blocker.CanMove(entity))
            return;

        TryStopPull(uid, component, user: uid);
    }

    private void OnPullableCollisionChange(EntityUid uid, PullableComponent component, ref CollisionChangeEvent args)
    {
        // IDK what this is supposed to be.
        if (!_timing.ApplyingState && component.PullJointId != null && !args.CanCollide)
        {
            _joints.RemoveJoint(uid, component.PullJointId);
        }
    }

    private void OnJointRemoved(EntityUid uid, PullableComponent component, JointRemovedEvent args)
    {
        // Just handles the joint getting nuked without going through pulling system (valid behavior).

        // Not relevant / pullable state handle it.
        if (component.Puller != args.OtherEntity ||
            args.Joint.ID != component.PullJointId ||
            _timing.ApplyingState)
        {
            return;
        }

        if (args.Joint.ID != component.PullJointId || component.Puller == null)
            return;

        StopPulling(uid, component);
    }

    /// <summary>
    /// Forces pulling to stop and handles cleanup.
    /// </summary>
    private void StopPulling(EntityUid pullableUid, PullableComponent pullableComp)
    {
        if (!_timing.ApplyingState)
        {
            if (TryComp<PhysicsComponent>(pullableUid, out var pullablePhysics))
            {
                _physics.SetFixedRotation(pullableUid, pullableComp.PrevFixedRotation, body: pullablePhysics);
            }
        }

        var oldPuller = pullableComp.Puller;
        pullableComp.PullJointId = null;
        pullableComp.Puller = null;
        pullableComp.GrabStage = GrabStage.No;
        pullableComp.GrabEscapeChance = 1f;
        _blocker.UpdateCanMove(pullableUid);

        Dirty(pullableUid, pullableComp);

        // No more joints with puller -> force stop pull.
        if (TryComp<PullerComponent>(oldPuller, out var pullerComp))
        {
            var pullerUid = oldPuller.Value;
            _alertsSystem.ClearAlert(pullerUid, AlertType.Pulling);
            pullerComp.Pulling = null;

            pullerComp.GrabStage = GrabStage.No;
            List<EntityUid> virtItems = pullerComp.GrabVirtualItems;
            foreach (var item in virtItems)
            {
                QueueDel(item);
            }
            pullerComp.GrabVirtualItems.Clear();

            Dirty(oldPuller.Value, pullerComp);

            // Messaging
            var message = new PullStoppedMessage(pullerUid, pullableUid);
            _modifierSystem.RefreshMovementSpeedModifiers(pullerUid);
            _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(pullerUid):user} stopped pulling {ToPrettyString(pullableUid):target}");

            RaiseLocalEvent(pullerUid, message);
            RaiseLocalEvent(pullableUid, message);
        }


        _alertsSystem.ClearAlert(pullableUid, AlertType.Pulled);
    }

    public bool IsPulled(EntityUid uid, PullableComponent? component = null)
    {
        return Resolve(uid, ref component, false) && component.BeingPulled;
    }

    private bool OnRequestMovePulledObject(ICommonSession? session, EntityCoordinates coords, EntityUid uid)
    {
        if (session?.AttachedEntity is not { } player ||
            !player.IsValid())
        {
            return false;
        }

        if (!TryComp<PullerComponent>(player, out var pullerComp))
            return false;

        var pulled = pullerComp.Pulling;

        if (!HasComp<PullableComponent>(pulled))
            return false;

        if (_containerSystem.IsEntityInContainer(player))
            return false;

        // Cooldown buddy
        if (_timing.CurTime < pullerComp.NextThrow)
            return false;

        pullerComp.NextThrow = _timing.CurTime + pullerComp.ThrowCooldown;

        // Cap the distance
        const float range = 2f;
        var fromUserCoords = coords.WithEntityId(player, EntityManager);
        var userCoords = new EntityCoordinates(player, Vector2.Zero);

        if (!userCoords.InRange(EntityManager, _xformSys, fromUserCoords, range))
        {
            var userDirection = fromUserCoords.Position - userCoords.Position;
            fromUserCoords = userCoords.Offset(userDirection.Normalized() * range);
        }

        Dirty(player, pullerComp);
        _throwing.TryThrow(pulled.Value, fromUserCoords, user: player, strength: 4f, animated: false, recoil: false, playSound: false, doSpin: false);
        return false;
    }

    public bool IsPulling(EntityUid puller, PullerComponent? component = null)
    {
        return Resolve(puller, ref component, false) && component.Pulling != null;
    }

    private void OnReleasePulledObject(ICommonSession? session)
    {
        if (session?.AttachedEntity is not {Valid: true} player)
        {
            return;
        }

        if (!TryComp(player, out PullerComponent? pullerComp) ||
            !TryComp(pullerComp.Pulling, out PullableComponent? pullableComp))
        {
            return;
        }

        TryStopPull(pullerComp.Pulling.Value, pullableComp, user: player, true);
    }

    public bool CanPull(EntityUid puller, EntityUid pullableUid, PullerComponent? pullerComp = null)
    {
        if (!Resolve(puller, ref pullerComp, false))
        {
            return false;
        }

        if (pullerComp.NeedsHands
            && !_handsSystem.TryGetEmptyHand(puller, out _)
            && pullerComp.Pulling == null)
        {
            return false;
        }

        if (!_blocker.CanInteract(puller, pullableUid))
        {
            return false;
        }

        if (!EntityManager.TryGetComponent<PhysicsComponent>(pullableUid, out var physics))
        {
            return false;
        }

        if (physics.BodyType == BodyType.Static)
        {
            return false;
        }

        if (puller == pullableUid)
        {
            return false;
        }

        if (!_containerSystem.IsInSameOrNoContainer(puller, pullableUid))
        {
            return false;
        }

        if (EntityManager.TryGetComponent(puller, out BuckleComponent? buckle))
        {
            // Prevent people pulling the chair they're on, etc.
            if (buckle is { PullStrap: false, Buckled: true } && (buckle.LastEntityBuckledTo == pullableUid))
            {
                return false;
            }
        }

        var getPulled = new BeingPulledAttemptEvent(puller, pullableUid);
        RaiseLocalEvent(pullableUid, getPulled, true);
        var startPull = new StartPullAttemptEvent(puller, pullableUid);
        RaiseLocalEvent(puller, startPull, true);
        return !startPull.Cancelled && !getPulled.Cancelled;
    }

    public bool TogglePull(EntityUid pullableUid, EntityUid pullerUid, PullableComponent pullable)
    {
        if (pullable.Puller != pullerUid)
            return TryStartPull(pullerUid, pullableUid, pullableComp: pullable);

        if (TryGrab((pullableUid, pullable), pullerUid))
            return true;

        if (!_combatMode.IsInCombatMode(pullableUid))
            return TryStopPull(pullableUid, pullable, ignoreGrab: true);

        return false;
    }

    public bool TogglePull(EntityUid pullerUid, PullerComponent puller)
    {
        if (!TryComp<PullableComponent>(puller.Pulling, out var pullable))
            return false;

        return TogglePull(puller.Pulling.Value, pullerUid, pullable);
    }

    public bool TryStartPull(EntityUid pullerUid, EntityUid pullableUid,
        PullerComponent? pullerComp = null, PullableComponent? pullableComp = null)
    {
        if (!Resolve(pullerUid, ref pullerComp, false) ||
            !Resolve(pullableUid, ref pullableComp, false))
        {
            return false;
        }

        if (pullerComp.Pulling == pullableUid)
            return true;

        if (!CanPull(pullerUid, pullableUid))
            return false;

        if (!EntityManager.TryGetComponent<PhysicsComponent>(pullerUid, out var pullerPhysics) ||
            !EntityManager.TryGetComponent<PhysicsComponent>(pullableUid, out var pullablePhysics))
        {
            return false;
        }

        // Ensure that the puller is not currently pulling anything.
        if (TryComp<PullableComponent>(pullerComp.Pulling, out var oldPullable)
            && !TryStopPull(pullerComp.Pulling.Value, oldPullable, pullerUid, true))
            return false;

        // Stop anyone else pulling the entity we want to pull
        if (pullableComp.Puller != null)
        {
            // We're already pulling this item
            if (pullableComp.Puller == pullerUid)
                return false;

            if (!TryStopPull(pullableUid, pullableComp, pullableComp.Puller))
            {
                // Not succeed to retake grabbed entity
                if (_netManager.IsServer)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-fail",
                            ("puller", Identity.Entity(pullableComp.Puller.Value, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullerUid, pullerUid, PopupType.MediumCaution);
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-fail-puller",
                            ("puller", Identity.Entity(pullerUid, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullableComp.Puller.Value, pullableComp.Puller.Value, PopupType.MediumCaution);
                }
                return false;
            }

            else if (pullableComp.GrabStage != GrabStage.No)
            {
                // Successful retake
                if (_netManager.IsServer)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-success",
                            ("puller", Identity.Entity(pullableComp.Puller.Value, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullerUid, pullerUid, PopupType.MediumCaution);
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-success-puller",
                            ("puller", Identity.Entity(pullerUid, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullableComp.Puller.Value, pullableComp.Puller.Value, PopupType.MediumCaution);
                }
            }
        }

        var pullAttempt = new PullAttemptEvent(pullerUid, pullableUid);
        RaiseLocalEvent(pullerUid, pullAttempt);

        if (pullAttempt.Cancelled)
            return false;

        RaiseLocalEvent(pullableUid, pullAttempt);

        if (pullAttempt.Cancelled)
            return false;

        // Pulling confirmed

        _interaction.DoContactInteraction(pullableUid, pullerUid);

        // Use net entity so it's consistent across client and server.
        pullableComp.PullJointId = $"pull-joint-{GetNetEntity(pullableUid)}";

        pullerComp.Pulling = pullableUid;
        pullableComp.Puller = pullerUid;

        // joint state handling will manage its own state
        if (!_timing.ApplyingState)
        {
            // Joint startup
            var union = _physics.GetHardAABB(pullerUid).Union(_physics.GetHardAABB(pullableUid, body: pullablePhysics));
            var length = Math.Max((float) union.Size.X, (float) union.Size.Y) * 0.75f;

            var joint = _joints.CreateDistanceJoint(pullableUid, pullerUid, id: pullableComp.PullJointId);
            joint.CollideConnected = false;
            // This maximum has to be there because if the object is constrained too closely, the clamping goes backwards and asserts.
            joint.MaxLength = Math.Max(1.0f, length);
            joint.Length = length * 0.75f;
            joint.MinLength = 0f;
            joint.Stiffness = 1f;

            _physics.SetFixedRotation(pullableUid, pullableComp.FixedRotationOnPull, body: pullablePhysics);
        }

        pullableComp.PrevFixedRotation = pullablePhysics.FixedRotation;

        // Messaging
        var message = new PullStartedMessage(pullerUid, pullableUid);
        _alertsSystem.ShowAlert(pullerUid, AlertType.Pulling, 0);
        _alertsSystem.ShowAlert(pullableUid, AlertType.Pulled, 0);

        RaiseLocalEvent(pullerUid, message);
        RaiseLocalEvent(pullableUid, message);

        Dirty(pullerUid, pullerComp);
        Dirty(pullableUid, pullableComp);

        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(pullerUid):user} started pulling {ToPrettyString(pullableUid):target}");

        if (_combatMode.IsInCombatMode(pullerUid))
            TryGrab(pullableUid, pullerUid);

        return true;
    }

    public bool TryStopPull(EntityUid pullableUid, PullableComponent pullable, EntityUid? user = null, bool ignoreGrab = false)
    {
        var pullerUidNull = pullable.Puller;

        if (pullerUidNull == null)
            return true;

        var msg = new AttemptStopPullingEvent(user);
        RaiseLocalEvent(pullableUid, msg, true);

        if (msg.Cancelled)
            return false;

        // There are some events that should ignore grab stages
        if (!ignoreGrab)
        {
            if (!AttemptGrabRelease(pullableUid))
            {
                if (_netManager.IsServer && user != null && user.Value == pullableUid)
                    _popup.PopupEntity(Loc.GetString("popup-grab-release-fail-self"), pullableUid, pullableUid, PopupType.SmallCaution);
                return false;
            }

            if (_netManager.IsServer && user != null && user.Value == pullableUid)
            {
                _popup.PopupEntity(Loc.GetString("popup-grab-release-success-self"), pullableUid, pullableUid, PopupType.SmallCaution);
                _popup.PopupEntity(Loc.GetString("popup-grab-release-success-puller", ("target", Identity.Entity(pullableUid, EntityManager))), pullerUidNull.Value, pullerUidNull.Value, PopupType.MediumCaution);
            }
        }

        // Stop pulling confirmed!
        if (!_timing.ApplyingState)
        {
            // Joint shutdown
            if (pullable.PullJointId != null)
            {
                _joints.RemoveJoint(pullableUid, pullable.PullJointId);
                pullable.PullJointId = null;
            }
        }

        StopPulling(pullableUid, pullable);
        return true;
    }

    /// <summary>
    /// Trying to grab the target
    /// </summary>
    /// <param name="pullable">Target that would be grabbed</param>
    /// <param name="puller">Performer of the grab</param>
    /// <param name="ignoreCombatMode">If true, will ignore disabled combat mode</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public bool TryGrab(Entity<PullableComponent?> pullable, Entity<PullerComponent?> puller, bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (!Resolve(puller.Owner, ref puller.Comp))
            return false;

        if (pullable.Comp.Puller != puller.Owner ||
            puller.Comp.Pulling != pullable.Owner)
            return false;

        if (puller.Comp.NextStageChange > _timing.CurTime)
            return true;

        // You can't choke crates
        if (!HasComp<MobStateComponent>(pullable))
            return false;

        // Delay to avoid spamming
        puller.Comp.NextStageChange = _timing.CurTime + puller.Comp.StageChangeCooldown;
        Dirty(puller);

        // Don't grab without grab intent
        if (!ignoreCombatMode)
        {
            if (!_combatMode.IsInCombatMode(puller.Owner))
                return false;
        }

        // It's blocking stage update, maybe better UX?
        if (puller.Comp.GrabStage == GrabStage.Suffocate)
        {
            _stamina.TakeStaminaDamage(pullable, puller.Comp.SuffocateGrabStaminaDamage);

            Dirty(pullable);
            Dirty(puller);
            return true;
        }

        // Update stage
        // TODO: Change grab stage direction
        var nextStageAddition = puller.Comp.GrabStageDirection switch
        {
            GrubStageDirection.Increase => 1,
            GrubStageDirection.Decrease => -1,
            _ => throw new ArgumentOutOfRangeException(),
        };

        var newStage = puller.Comp.GrabStage + nextStageAddition;

        if (!TrySetGrabStages((puller.Owner, puller.Comp), (pullable.Owner, pullable.Comp), newStage))
            return false;

        _color.RaiseEffect(Color.Yellow, new List<EntityUid> { pullable }, Filter.Pvs(pullable, entityManager: EntityManager));
        return true;
    }

    private bool TrySetGrabStages(Entity<PullerComponent> puller, Entity<PullableComponent> pullable, GrabStage stage)
    {
        puller.Comp.GrabStage = stage;
        pullable.Comp.GrabStage = stage;

        if (!TryUpdateGrabVirtualItems(puller, pullable))
            return false;

        var filter = Filter.Empty()
            .AddPlayersByPvs(Transform(puller))
            .RemovePlayerByAttachedEntity(puller.Owner)
            .RemovePlayerByAttachedEntity(pullable.Owner);

        var popupType = stage switch
        {
            GrabStage.No => PopupType.Small,
            GrabStage.Soft => PopupType.Small,
            GrabStage.Hard => PopupType.MediumCaution,
            GrabStage.Suffocate => PopupType.LargeCaution,
            _ => throw new ArgumentOutOfRangeException()
        };

        pullable.Comp.GrabEscapeChance = puller.Comp.EscapeChances[stage];

        _alertsSystem.ShowAlert(puller, AlertType.Pulling, puller.Comp.PullingAlertSeverity[stage]);
        _alertsSystem.ShowAlert(pullable, AlertType.Pulled, pullable.Comp.PulledAlertAlertSeverity[stage]);

        _blocker.UpdateCanMove(pullable);
        _modifierSystem.RefreshMovementSpeedModifiers(puller);

        // I'm lazy to write client code
        if (!_netManager.IsServer)
            return true;

        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-target", ("puller", Identity.Entity(puller, EntityManager))), pullable, pullable, popupType);
        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-self", ("target", Identity.Entity(pullable, EntityManager))), pullable, puller, PopupType.Medium);
        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-others", ("target", Identity.Entity(pullable, EntityManager)), ("puller", Identity.Entity(puller, EntityManager))), pullable, filter, true, popupType);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), pullable);

        Dirty(pullable);
        Dirty(puller);

        return true;
    }

    private bool TryUpdateGrabVirtualItems(Entity<PullerComponent> puller, Entity<PullableComponent> pullable)
    {
        // Updating virtual items
        var virtualItemsCount = puller.Comp.GrabVirtualItems.Count;

        var newVirtualItemsCount = puller.Comp.NeedsHands ? 0 : 1;
        if (puller.Comp.GrabVirtualItemStageCount.TryGetValue(puller.Comp.GrabStage, out var count))
            newVirtualItemsCount += count;

        if (virtualItemsCount != newVirtualItemsCount)
        {
            var delta = newVirtualItemsCount - virtualItemsCount;

            // Adding new virtual items
            if (delta > 0)
            {
                for (var i = 0; i < delta; i++)
                {
                    if (!_virtualSystem.TrySpawnVirtualItemInHand(pullable, puller.Owner, out var item, true))
                    {
                        // I'm lazy write client code
                        if (_netManager.IsServer)
                            _popup.PopupEntity(Loc.GetString("popup-grab-need-hand"), puller, puller, PopupType.Medium);

                        return false;
                    }

                    puller.Comp.GrabVirtualItems.Add(item.Value);
                }
            }

            if (delta < 0)
            {
                for (var i = 0; i < Math.Abs(delta); i++)
                {
                    if (i >= puller.Comp.GrabVirtualItems.Count)
                        break;

                    var item = puller.Comp.GrabVirtualItems[i];
                    puller.Comp.GrabVirtualItems.Remove(item);
                    QueueDel(item);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Attempts to release entity from grab
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <returns></returns>
    public bool AttemptGrabRelease(Entity<PullableComponent?> pullable)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;
        if (_timing.CurTime < pullable.Comp.NextEscapeAttempt)  // No autoclickers! Mwa-ha-ha
        {
            return false;
        }

        if (_random.Prob(pullable.Comp.GrabEscapeChance))
            return true;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(1));
        Dirty(pullable.Owner, pullable.Comp);
        return false;
    }

    private void OnGrabbedMoveAttempt(EntityUid uid, PullableComponent component, UpdateCanMoveEvent args)
    {
        if (component.GrabStage == GrabStage.No)
            return;

        args.Cancel();

    }

    private void OnGrabbedSpeakAttempt(EntityUid uid, PullableComponent component, SpeakAttemptEvent args)
    {
        if (component.GrabStage != GrabStage.Suffocate)
            return;

        _popup.PopupEntity(Loc.GetString("popup-grabbed-cant-speak"), uid, uid, PopupType.MediumCaution);   // You cant speak while someone is choking you

        args.Cancel();
    }

    /// <summary>
    /// Tries to lower grab stage for target or release it
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <param name="puller">Performer</param>
    /// <param name="ignoreCombatMode">If true, will NOT release target if combat mode is off</param>
    /// <returns></returns>
    public bool TryLowerGrabStage(Entity<PullableComponent?> pullable, Entity<PullerComponent?> puller, bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (!Resolve(puller.Owner, ref puller.Comp))
            return false;

        if (pullable.Comp.Puller != puller.Owner ||
            puller.Comp.Pulling != pullable.Owner)
            return false;

        if (_timing.CurTime < puller.Comp.NextStageChange)
            return true;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(1f));
        Dirty(pullable);

        if (!ignoreCombatMode && _combatMode.IsInCombatMode(puller.Owner))
        {
            TryStopPull(pullable, pullable.Comp, ignoreGrab: true);
            return true;
        }

        if (puller.Comp.GrabStage == GrabStage.No)
        {
            TryStopPull(pullable, pullable.Comp, ignoreGrab: true);
            return true;
        }

        var newStage = puller.Comp.GrabStage - 1;
        TrySetGrabStages((puller.Owner, puller.Comp), (pullable.Owner, pullable.Comp), newStage);
        return true;
    }
}

public enum GrabStage
{
    No = 0,
    Soft = 1,
    Hard = 2,
    Suffocate = 3,
}

public enum GrubStageDirection
{
    Increase,
    Decrease,
}
