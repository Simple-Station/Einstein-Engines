using System.Diagnostics.CodeAnalysis;
using Content.Shared.Verbs;
using Content.Shared.Throwing;
using Content.Shared.Movement;
using Content.Shared.Movement.Events;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using System.Linq;
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
        SubscribeLocalEvent<StrapComponent, ComponentStartup>(OnStrapStartup);
        SubscribeLocalEvent<StrapComponent, ComponentShutdown>(OnStrapShutdown);
    }

    private void InitializeBuckle()
    {
        SubscribeLocalEvent<BuckleComponent, ComponentStartup>(OnBuckleStartup);
        SubscribeLocalEvent<BuckleComponent, ComponentShutdown>(OnBuckleShutdown);
        SubscribeLocalEvent<BuckleComponent, MoveEvent>(OnBuckleMove);
        SubscribeLocalEvent<BuckleComponent, InteractHandEvent>(OnBuckleInteractHand);
        SubscribeLocalEvent<BuckleComponent, GetVerbsEvent<InteractionVerb>>(AddUnbuckleVerb);
        SubscribeLocalEvent<BuckleComponent, DownAttemptEvent>(OnBuckleDownAttempt);
        SubscribeLocalEvent<BuckleComponent, StandAttemptEvent>(OnBuckleStandAttempt);
        SubscribeLocalEvent<BuckleComponent, ThrowPushbackAttemptEvent>(OnBuckleThrowPushbackAttempt);
        SubscribeLocalEvent<BuckleComponent, UpdateCanMoveEvent>(OnBuckleUpdateCanMove);
    }

    public bool ToggleBuckle(EntityUid buckleUid, EntityUid userUid, EntityUid strapUid, BuckleComponent? buckleComp = null, StrapComponent? strapComp = null)
    {
        if (!Resolve(buckleUid, ref buckleComp, false) || !Resolve(strapUid, ref strapComp, false))
            return false;

        if (buckleComp.Buckled)
            return TryUnbuckle(buckleUid, userUid, false, buckleComp, strapComp);
        else
            return TryBuckle(buckleUid, userUid, strapUid, buckleComp, strapComp);
    }

    public bool IsBuckled(EntityUid uid, BuckleComponent? buckleComp = null)
    {
        if (!Resolve(uid, ref buckleComp, false))
            return false;

        return buckleComp.Buckled;
    }

    public void StrapSetEnabled(EntityUid uid, bool enabled)
    {
        if (TryComp<StrapComponent>(uid, out var strap))
        {
            strap.Enabled = enabled;
            Dirty(uid, strap);
        }
    }

    private void OnStrapStartup(EntityUid uid, StrapComponent component, ComponentStartup args)
    {
        UpdateStrapVisuals(uid, component);

        var buckleQuery = GetEntityQuery<BuckleComponent>();
        var xform = Transform(uid);
        var buckleEntities = GetEntitiesInRange(uid, component.MaxBuckleDistance);

        foreach (var entity in buckleEntities)
        {
            if (buckleQuery.TryGetComponent(entity, out var buckle) && !buckle.Buckled)
            {
                TryBuckle(entity, entity, uid, buckle, component);
            }
        }
    }

    public IEnumerable<EntityUid> GetEntitiesInRange(EntityUid uid, float range, TransformComponent? xform = null)
    {
        if (!Resolve(uid, ref xform))
            yield break;

        var xformQuery = GetEntityQuery<TransformComponent>();
        var worldPos = _transform.GetWorldPosition(xform);
        var rangeSquared = range * range;

        var query = EntityQueryEnumerator<TransformComponent>();
        while (query.MoveNext(out var otherUid, out var otherXform))
        {
            if (otherUid == uid)
                continue;

            var otherWorldPos = _transform.GetWorldPosition(otherXform, xformQuery);
            var displacement = otherWorldPos - worldPos;

            if (displacement.LengthSquared() <= rangeSquared)
            {
                yield return otherUid;
            }
        }
    }

    private void OnStrapShutdown(EntityUid uid, StrapComponent component, ComponentShutdown args)
    {
        // Unbuckle all entities from this strap
        foreach (var buckledEntity in component.BuckledEntities.ToList())
        {
            TryUnbuckle(buckledEntity, buckledEntity, true);
        }

        // Clear the buckled entities list
        component.BuckledEntities.Clear();
        component.OccupiedSize = 0;

        // Update the strap's visual state
        UpdateStrapVisuals(uid, component);
    }

    private void UpdateStrapVisuals(EntityUid uid, StrapComponent? strap = null)
    {
        if (!Resolve(uid, ref strap))
            return;

        Appearance.SetData(uid, StrapVisuals.State, strap.BuckledEntities.Count > 0);
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

        var bucklePos = _transform.GetWorldPosition(uid);
        var strapPos = _transform.GetWorldPosition(component.BuckledTo.Value);

        if ((bucklePos - strapPos).LengthSquared() > component.Range * component.Range)
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

        if (!TryComp<StrapComponent>(component.BuckledTo.Value, out var strap))
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
        if (!Resolve(buckleUid, ref buckleComp, false) || !buckleComp.Buckled || buckleComp.BuckledTo == null || !Resolve(buckleComp.BuckledTo.Value, ref strapComp, false))
            return false;

        if (!force && _gameTiming.CurTime < buckleComp.BuckleTime + buckleComp.Delay)
            return false;

        buckleComp.Buckled = false;
        buckleComp.BuckledTo = null!;

        if (_mobState.IsIncapacitated(buckleUid))
            _standing.Down(buckleUid);

        if (strapComp.BuckledEntities.Remove(buckleUid))
        {
            strapComp.OccupiedSize -= buckleComp.Size;
            Dirty(buckleComp.BuckledTo.Value, strapComp);
        }

        _joints.RefreshRelay(buckleUid);
        Appearance.SetData(buckleComp.BuckledTo.Value, StrapVisuals.State, strapComp.BuckledEntities.Count != 0);

        if (!TerminatingOrDeleted(buckleComp.BuckledTo.Value))
            _audio.PlayPredicted(strapComp.UnbuckleSound, buckleComp.BuckledTo.Value, userUid);

        var ev = new BuckleChangeEvent(buckleComp.BuckledTo.Value, buckleUid, false);
        RaiseLocalEvent(buckleUid, ref ev);
        RaiseLocalEvent(buckleComp.BuckledTo.Value, ref ev);

        return true;
    }

    private bool CanBuckle(EntityUid buckleUid, EntityUid userUid, EntityUid strapUid, [NotNullWhen(true)] out StrapComponent? strapComp, BuckleComponent? buckleComp = null)
    {
        strapComp = null;

        if (!Resolve(buckleUid, ref buckleComp, false) || !Resolve(strapUid, ref strapComp, false))
            return false;

        if (userUid == strapUid)
            return false;

        if (!ActionBlocker.CanInteract(userUid, strapUid))
            return false;

        if (!IsEntityAllowed(buckleUid, strapComp))
            return false;

        if (!_interaction.InRangeUnobstructed(buckleUid, strapUid, buckleComp.Range))
            return false;

        if (strapComp.OccupiedSize + buckleComp.Size > strapComp.Size)
            return false;

        if (!strapComp.Enabled)
            return false;

        return true;
    }

    private bool IsEntityAllowed(EntityUid buckleUid, StrapComponent strapComp)
    {
        return strapComp.AllowedEntities == null || strapComp.AllowedEntities.IsValid(buckleUid, EntityManager);
    }

    private bool IsInRange(EntityUid buckleUid, EntityUid strapUid, float range)
    {
        return _interaction.InRangeUnobstructed(buckleUid, strapUid, range);
    }

    private void ReAttach(EntityUid buckleUid, EntityUid strapUid, BuckleComponent? buckleComp = null, StrapComponent? strapComp = null)
    {
        if (!Resolve(strapUid, ref strapComp, false) || !Resolve(buckleUid, ref buckleComp, false))
            return;

        _transform.SetCoordinates(buckleUid, new EntityCoordinates(strapUid, strapComp.BuckleOffset));

        var buckleTransform = Transform(buckleUid);

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

    public enum StrapVisuals
    {
        State
    }
}
