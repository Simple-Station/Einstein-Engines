using System.Diagnostics.CodeAnalysis;
using Content.Shared.Verbs;
using Content.Shared.Physics;
using Content.Shared.Throwing;
using Content.Shared.Movement;
using Content.Shared.Movement.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Pulling;
using Content.Shared.Standing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.Buckle;

public abstract partial class SharedBuckleSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    [Dependency] protected readonly ActionBlockerSystem ActionBlocker = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;

    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SharedInteractionSystem));
        UpdatesAfter.Add(typeof(SharedInputSystem));

        InitializeBuckle();
        InitializeStrap();
    }

    private void InitializeStrap()
    {
        // Initialization logic for straps...
    }

    private void InitializeBuckle()
    {
        SubscribeLocalEvent<BuckleComponent, ComponentStartup>(OnBuckleStartup);
        SubscribeLocalEvent<BuckleComponent, ComponentShutdown>(OnBuckleShutdown);
        SubscribeLocalEvent<BuckleComponent, MoveEvent>(OnBuckleMove);
        SubscribeLocalEvent<BuckleComponent, InteractHandEvent>(OnBuckleInteractHand);
        SubscribeLocalEvent<BuckleComponent, GetVerbsEvent<InteractionVerb>>(AddUnbuckleVerb);
        SubscribeLocalEvent<BuckleComponent, PreventCollideEvent>(OnBucklePreventCollide);
        SubscribeLocalEvent<BuckleComponent, DownAttemptEvent>(OnBuckleDownAttempt);
        SubscribeLocalEvent<BuckleComponent, StandAttemptEvent>(OnBuckleStandAttempt);
        SubscribeLocalEvent<BuckleComponent, ThrowPushbackAttemptEvent>(OnBuckleThrowPushbackAttempt);
        SubscribeLocalEvent<BuckleComponent, UpdateCanMoveEvent>(OnBuckleUpdateCanMove);
    }

    private void OnBuckleStartup(EntityUid uid, BuckleComponent component, ComponentStartup args)
    {
        UpdateBuckleStatus(uid, component);
    }

    private void OnBuckleShutdown(EntityUid uid, BuckleComponent component, ComponentShutdown args)
    {
        TryUnbuckle(uid, uid, true, component);
    }

    private void OnBuckleMove(EntityUid uid, BuckleComponent component, ref MoveEvent args)
    {
        if (component.BuckledTo == null || component.BuckledTo == uid)
            return;

        if (!_transform.GetWorldPosition(uid).InRange(_transform.GetWorldPosition(component.BuckledTo.Value), component.Range))
            TryUnbuckle(uid, uid, true, component);
    }

    private void OnBuckleInteractHand(EntityUid uid, BuckleComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (component.Buckled)
            TryUnbuckle(uid, args.User, false, component);
        else
            TryBuckle(uid, args.User, args.Target, component);

        args.Handled = true;
    }

    private void AddUnbuckleVerb(EntityUid uid, BuckleComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (!component.Buckled)
            return;

        InteractionVerb verb = new()
        {
            Text = "Unbuckle",
            Act = () => TryUnbuckle(uid, args.User, false, component),
            Category = VerbCategory.Unbuckle
        };

        args.Verbs.Add(verb);
    }

    private void OnBucklePreventCollide(EntityUid uid, BuckleComponent component, PreventCollideEvent args)
    {
        if (component.BuckledTo == null)
            return;

        if (args.OtherEntity == component.BuckledTo)
            args.Cancel();
    }

    private void OnBuckleDownAttempt(EntityUid uid, BuckleComponent component, DownAttemptEvent args)
    {
        if (component.Buckled)
            args.Cancel();
    }

    private void OnBuckleStandAttempt(EntityUid uid, BuckleComponent component, StandAttemptEvent args)
    {
        if (component.Buckled)
            args.Cancel();
    }

    private void OnBuckleThrowPushbackAttempt(EntityUid uid, BuckleComponent component, ThrowPushbackAttemptEvent args)
    {
        if (component.Buckled)
            args.Cancel();
    }

    private void OnBuckleUpdateCanMove(EntityUid uid, BuckleComponent component, UpdateCanMoveEvent args)
    {
        if (component.Buckled)
            args.Cancel();
    }

    private void UpdateBuckleStatus(EntityUid uid, BuckleComponent component)
    {
        if (component.BuckledTo == null)
            return;

        var strap = Comp<StrapComponent>(component.BuckledTo.Value);
        if (strap == null)
            return;

        if (strap.BuckledEntities.Contains(uid))
            return;

        strap.BuckledEntities.Add(uid);
        strap.OccupiedSize += component.Size;
        Dirty(component.BuckledTo.Value, strap);
    }

    public bool TryBuckle(EntityUid buckleUid, EntityUid userUid, EntityUid strapUid, BuckleComponent? buckleComp = null, StrapComponent? strapComp = null)
    {
        if (!Resolve(buckleUid, ref buckleComp, false) || !Resolve(strapUid, ref strapComp, false))
            return false;

        if (buckleComp.Buckled || !CanBuckle(buckleUid, userUid, strapUid, out strapComp, buckleComp))
            return false;

        buckleComp.Buckled = true;
        buckleComp.BuckledTo = strapUid;
        buckleComp.BuckleTime = _gameTiming.CurTime;

        strapComp.BuckledEntities.Add(buckleUid);
        strapComp.OccupiedSize += buckleComp.Size;

        ReAttach(buckleUid, strapUid, buckleComp, strapComp);

        _audio.PlayPredicted(strapComp.BuckleSound, strapUid, userUid);
        _alerts.ShowAlert(buckleUid, strapComp.BuckledAlertType);

        var ev = new BuckleChangeEvent(strapUid, buckleUid, true);
        RaiseLocalEvent(buckleUid, ref ev);
        RaiseLocalEvent(strapUid, ref ev);

        return true;
    }

    public bool TryUnbuckle(EntityUid buckleUid, EntityUid userUid, bool force, BuckleComponent? buckleComp = null, StrapComponent? strapComp = null)
    {
        if (!Resolve(buckleUid, ref buckleComp, false) || !buckleComp.Buckled || !Resolve(buckleComp.BuckledTo, ref strapComp, false))
            return false;

        if (!force && _gameTiming.CurTime < buckleComp.BuckleTime + buckleComp.Delay)
            return false;

        buckleComp.Buckled = false;
        buckleComp.BuckledTo = null;

        if (_mobState.IsIncapacitated(buckleUid))
            _standing.Down(buckleUid);

        if (strapComp.BuckledEntities.Remove(buckleUid))
        {
            strapComp.OccupiedSize -= buckleComp.Size;
            Dirty(strapComp.Owner, strapComp);
        }

        _joints.RefreshRelay(buckleUid);
        Appearance.SetData(strapComp.Owner, StrapVisuals.State, strapComp.BuckledEntities.Count != 0);

        if (!TerminatingOrDeleted(strapComp.Owner))
            _audio.PlayPredicted(strapComp.UnbuckleSound, strapComp.Owner, userUid);

        var ev = new BuckleChangeEvent(strapComp.Owner, buckleUid, false);
        RaiseLocalEvent(buckleUid, ref ev);
        RaiseLocalEvent(strapComp.Owner, ref ev);

        return true;
    }

    private bool CanBuckle(EntityUid buckleUid, EntityUid userUid, EntityUid strapUid, [NotNullWhen(true)] out StrapComponent? strapComp, BuckleComponent? buckleComp = null)
    {
        strapComp = null;

        // Guard clauses
        if (userUid == strapUid || !Resolve(buckleUid, ref buckleComp, false) || !Resolve(strapUid, ref strapComp, false))
            return false;

        if (!IsAllowedBuckleType(buckleComp, userUid))
            return false;

        if (!IsStrapEnabled(strapComp))
            return false;

        if (!IsStrapSizeValid(buckleComp, strapComp))
            return false;

        if (!IsInRange(buckleUid, strapUid, buckleComp))
            return false;

        if (!IsEntityAllowed(buckleUid, strapComp))
            return false;

        return true;
    }

    private bool IsAllowedBuckleType(BuckleComponent? buckleComp, EntityUid userUid)
    {
        if (buckleComp?.AllowedBuckleTypes != null && !buckleComp.AllowedBuckleTypes.IsValid(userUid, EntityManager))
        {
            if (_netManager.IsServer)
                _popup.PopupEntity(Loc.GetString("buckle-component-cannot-fit-message"), userUid, buckleUid, PopupType.Medium);
            return false;
        }

        return true;
    }

    private bool IsStrapEnabled(StrapComponent? strapComp)
        => strapComp?.Enabled == true;

    private bool IsStrapSizeValid(BuckleComponent? buckleComp, StrapComponent? strapComp)
        => buckleComp != null && strapComp != null && strapComp.OccupiedSize + buckleComp.Size <= strapComp.Size;

    private bool IsInRange(EntityUid buckleUid, EntityUid strapUid, BuckleComponent? buckleComp)
        => buckleComp != null && _interaction.InRangeUnobstructed(buckleUid, strapUid, buckleComp.Range);

    private bool IsEntityAllowed(EntityUid buckleUid, StrapComponent? strapComp)
        => strapComp?.AllowedEntities == null || strapComp.AllowedEntities.IsValid(buckleUid);

    private void ReAttach(EntityUid buckleUid, EntityUid strapUid, BuckleComponent? buckleComp = null, StrapComponent? strapComp = null)
    {
        if (!Resolve(strapUid, ref strapComp, false) || !Resolve(buckleUid, ref buckleComp, false))
            return;

        _transform.SetCoordinates(buckleUid, new EntityCoordinates(strapUid, strapComp.BuckleOffsetClamped));

        var buckleTransform = Transform(buckleUid);

        // Buckle subscribes to move for < reasons > so this might fail.
        // TODO: Make buckle not do that.

        if (buckleTransform.ParentUid != strapUid)
            return;

        _transform.SetLocalRotation(buckleUid, Angle.Zero, buckleTransform);
        _joints.SetRelay(buckleUid, strapUid);

        switch (strapComp.Position)
        {
            case StrapPosition.None:
                break;
            case StrapPosition.Stand:
                _standing.Stand(buckleUid);
                break;
            case StrapPosition.Down:
                _standing.Down(buckleUid, false, false);
                break;
        }
    }
}
