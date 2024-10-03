<<<<<<< HEAD:Content.Shared/Standing/SharedLayingDownSystem.cs
using Content.Shared.ActionBlocker;
using Content.Shared.CCVar;
||||||| parent of b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
=======
using Content.Shared.Backmen.CCVar;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
>>>>>>> b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
using Content.Shared.DoAfter;
using Content.Shared.Gravity;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Body.Components;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Standing;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
<<<<<<< HEAD:Content.Shared/Standing/SharedLayingDownSystem.cs
using Robust.Shared.Configuration;
||||||| parent of b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
using Robust.Shared.Containers;
=======
using Content.Shared.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
>>>>>>> b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Content.Shared.Movement.Components;

namespace Content.Shared.Standing;

public abstract class SharedLayingDownSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
<<<<<<< HEAD:Content.Shared/Standing/SharedLayingDownSystem.cs
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
||||||| parent of b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
=======
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly IConfigurationManager _config = default!;

    protected bool CrawlUnderTables = false;
>>>>>>> b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs

    public override void Initialize()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ToggleStanding, InputCmdHandler.FromDelegate(ToggleStanding))
            .Bind(ContentKeyFunctions.ToggleCrawlingUnder, InputCmdHandler.FromDelegate(HandleCrawlUnderRequest, handle: false))
            .Register<SharedLayingDownSystem>();

        SubscribeNetworkEvent<ChangeLayingDownEvent>(OnChangeState);

        SubscribeLocalEvent<StandingStateComponent, StandingUpDoAfterEvent>(OnStandingUpDoAfter);
        SubscribeLocalEvent<LayingDownComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<LayingDownComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<LayingDownComponent, MobStateChangedEvent>(OnChangeMobState);

        SubscribeLocalEvent<LayingDownComponent, BuckledEvent>(OnBuckled);
        SubscribeLocalEvent<LayingDownComponent, UnbuckledEvent>(OnUnBuckled);
        SubscribeLocalEvent<BoundUserInterfaceMessageAttempt>(OnBoundUserInterface, after: [typeof(SharedInteractionSystem)]);

        Subs.CVar(_config, CCVars.CrawlUnderTables, b => CrawlUnderTables = b, true);
    }

    private void OnBoundUserInterface(BoundUserInterfaceMessageAttempt args)
    {
        if(
            args.Cancelled ||
            !TryComp<ActivatableUIComponent>(args.Target, out var uiComp) ||
            !TryComp<StandingStateComponent>(args.Actor, out var standingStateComponent) ||
            standingStateComponent.CurrentState != StandingState.Lying)
            return;

        if(uiComp.RequiresComplex)
            args.Cancel();
    }

    private void OnChangeMobState(Entity<LayingDownComponent> ent, ref MobStateChangedEvent args)
    {
        if(
            !TryComp<StandingStateComponent>(ent, out var standingStateComponent) ||
            standingStateComponent.CurrentState != StandingState.Lying)
            return;

        if (args.NewMobState == MobState.Alive)
        {
            AutoGetUp(ent);
            TryStandUp(ent, ent, standingStateComponent);
            return;
        }

        if (CrawlUnderTables)
        {
            if(_net.IsServer)
                RaiseNetworkEvent(new DrawUpEvent(GetNetEntity(ent)), Filter.PvsExcept(ent));
            else
                RaiseLocalEvent(new DrawUpEvent(GetNetEntity(ent)));
        }
    }



    private void OnUnBuckled(Entity<LayingDownComponent> ent, ref UnbuckledEvent args)
    {
        if(
            !TryComp<StandingStateComponent>(ent, out var standingStateComponent) ||
            standingStateComponent.CurrentState != StandingState.Lying)
            return;

        if (CrawlUnderTables)
        {
            if(_net.IsServer)
                RaiseNetworkEvent(new DrawDownedEvent(GetNetEntity(ent)), Filter.PvsExcept(ent));
            else
                RaiseLocalEvent(new DrawDownedEvent(GetNetEntity(ent)));
        }
    }

    private void OnBuckled(Entity<LayingDownComponent> ent, ref BuckledEvent args)
    {
        if(
            !TryComp<StandingStateComponent>(ent, out var standingStateComponent) ||
            standingStateComponent.CurrentState != StandingState.Lying)
            return;

        if (CrawlUnderTables)
        {
            if(_net.IsServer)
                RaiseNetworkEvent(new DrawUpEvent(GetNetEntity(ent)), Filter.Pvs(ent));
            else
                RaiseLocalEvent(new DrawUpEvent(GetNetEntity(ent)));
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedLayingDownSystem>();
    }

    private void ToggleStanding(ICommonSession? session)
    {
        if (session is not { AttachedEntity: { Valid: true } uid } _
            || !Exists(uid)
            || !HasComp<LayingDownComponent>(session.AttachedEntity)
            || _gravity.IsWeightless(session.AttachedEntity.Value))
            return;

        if (_net.IsServer)
        {
            RaiseNetworkEvent(new ChangeLayingDownEvent(), Filter.Pvs(session.AttachedEntity.Value));
        }
        else
        {
            RaiseNetworkEvent(new ChangeLayingDownEvent());
        }

    }

    private void HandleCrawlUnderRequest(ICommonSession? session)
    {
        if (session == null
            || session.AttachedEntity is not {} uid
            || !TryComp<StandingStateComponent>(uid, out var standingState)
            || !TryComp<LayingDownComponent>(uid, out var layingDown)
            || !_actionBlocker.CanInteract(uid, null))
            return;

        var newState = !layingDown.IsCrawlingUnder;
        if (standingState.CurrentState is StandingState.Standing)
            newState = false; // If the entity is already standing, this function only serves a fallback method to fix its draw depth

        // Do not allow to begin crawling under if it's disabled in config. We still, however, allow to stop it, as a failsafe.
        if (newState && !_config.GetCVar(CCVars.CrawlUnderTables))
        {
            _popups.PopupEntity(Loc.GetString("crawling-under-tables-disabled-popup"), uid, session);
            return;
        }

        layingDown.IsCrawlingUnder = newState;
        _speed.RefreshMovementSpeedModifiers(uid);
        Dirty(uid, layingDown);
    }

    private void OnChangeState(ChangeLayingDownEvent ev, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue)
            return;

        var uid = args.SenderSession.AttachedEntity.Value;
        if (!TryComp(uid, out StandingStateComponent? standing)
            || !TryComp(uid, out LayingDownComponent? layingDown))
            return;

        RaiseNetworkEvent(new CheckAutoGetUpEvent(GetNetEntity(uid)));

        if (HasComp<KnockedDownComponent>(uid)
            || !_mobState.IsAlive(uid))
            return;

        if (_standing.IsDown(uid, standing))
            TryStandUp(uid, layingDown, standing);
        else
            TryLieDown(uid, layingDown, standing);
    }

    private void OnStandingUpDoAfter(EntityUid uid, StandingStateComponent component, StandingUpDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled
            || HasComp<KnockedDownComponent>(uid)
            || _mobState.IsIncapacitated(uid)
            || !_standing.Stand(uid))
            component.CurrentState = StandingState.Lying;

        component.CurrentState = StandingState.Standing;
    }

    private void OnRefreshMovementSpeed(EntityUid uid, LayingDownComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!_standing.IsDown(uid))
            return;

        var modifier = component.LyingSpeedModifier * (component.IsCrawlingUnder ? component.CrawlingUnderSpeedModifier : 1);
        args.ModifySpeed(modifier, modifier);
    }

    private void OnParentChanged(EntityUid uid, LayingDownComponent component, EntParentChangedMessage args)
    {
        // If the entity is not on a grid, try to make it stand up to avoid issues
        if (!TryComp<StandingStateComponent>(uid, out var standingState)
            || standingState.CurrentState is StandingState.Standing
            || Transform(uid).GridUid != null)
            return;

        _standing.Stand(uid, standingState);
    }

    public bool TryStandUp(EntityUid uid, LayingDownComponent? layingDown = null, StandingStateComponent? standingState = null)
    {
<<<<<<< HEAD:Content.Shared/Standing/SharedLayingDownSystem.cs
        if (!Resolve(uid, ref standingState, false)
            || !Resolve(uid, ref layingDown, false)
            || standingState.CurrentState is not StandingState.Lying
            || !_mobState.IsAlive(uid)
            || TerminatingOrDeleted(uid)
            || !TryComp<BodyComponent>(uid, out var body)
            || body.LegEntities.Count < body.RequiredLegs
            || HasComp<DebrainedComponent>(uid)
            || TryComp<MovementSpeedModifierComponent>(uid, out var movement) && movement.CurrentWalkSpeed == 0)
||||||| parent of b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
        if (!Resolve(uid, ref standingState, false) ||
            !Resolve(uid, ref layingDown, false) ||
            standingState.CurrentState is not StandingState.Lying ||
            !_mobState.IsAlive(uid) ||
            TerminatingOrDeleted(uid))
        {
=======
        if (!Resolve(uid, ref standingState, false) ||
            !Resolve(uid, ref layingDown, false) ||
            standingState.CurrentState is not StandingState.Lying ||
            !_mobState.IsAlive(uid) ||
            _buckle.IsBuckled(uid) ||
            TerminatingOrDeleted(uid))
        {
>>>>>>> b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
            return false;

        var args = new DoAfterArgs(EntityManager, uid, layingDown.StandingUpTime, new StandingUpDoAfterEvent(), uid)
        {
            BreakOnHandChange = false,
            RequireCanInteract = false
        };

        if (!_doAfter.TryStartDoAfter(args))
            return false;

        standingState.CurrentState = StandingState.GettingUp;
        layingDown.IsCrawlingUnder = false;
        return true;
    }

    public bool TryLieDown(EntityUid uid, LayingDownComponent? layingDown = null, StandingStateComponent? standingState = null, DropHeldItemsBehavior behavior = DropHeldItemsBehavior.NoDrop)
    {
<<<<<<< HEAD:Content.Shared/Standing/SharedLayingDownSystem.cs
        if (!Resolve(uid, ref standingState, false)
            || !Resolve(uid, ref layingDown, false)
            || standingState.CurrentState is not StandingState.Standing)
||||||| parent of b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
        if (!Resolve(uid, ref standingState, false) ||
            !Resolve(uid, ref layingDown, false) ||
            standingState.CurrentState is not StandingState.Standing)
=======
        if (!Resolve(uid, ref standingState, false) ||
            !Resolve(uid, ref layingDown, false) ||
            standingState.CurrentState is not StandingState.Standing ||
            _buckle.IsBuckled(uid))
>>>>>>> b0a9653c83 ([Fix] Fix laydown fix blob  (#815)):Content.Shared/Backmen/Standing/SharedLayingDownSystem.cs
        {
            if (behavior == DropHeldItemsBehavior.AlwaysDrop)
                RaiseLocalEvent(uid, new DropHandItemsEvent());

            return false;
        }

        _standing.Down(uid, true, behavior != DropHeldItemsBehavior.NoDrop, standingState);
        return true;
    }
}

[Serializable, NetSerializable]
public sealed partial class StandingUpDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum DropHeldItemsBehavior : byte
{
    NoDrop,
    DropIfStanding,
    AlwaysDrop
}
