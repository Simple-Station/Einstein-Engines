using Content.Shared.ActionBlocker;
using Content.Shared.CCVar;
using Content.Shared.DoAfter;
using Content.Shared.Gravity;
using Content.Shared.Input;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Configuration;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared.Standing;

public abstract class SharedLayingDownSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

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

        RaiseNetworkEvent(new ChangeLayingDownEvent());
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
        if (!Resolve(uid, ref standingState, false)
            || !Resolve(uid, ref layingDown, false)
            || standingState.CurrentState is not StandingState.Lying
            || !_mobState.IsAlive(uid)
            || TerminatingOrDeleted(uid))
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
        if (!Resolve(uid, ref standingState, false)
            || !Resolve(uid, ref layingDown, false)
            || standingState.CurrentState is not StandingState.Standing)
        {
            if (behavior == DropHeldItemsBehavior.AlwaysDrop)
                RaiseLocalEvent(uid, new DropHandItemsEvent());

            return false;
        }

        _standing.Down(uid, true, behavior != DropHeldItemsBehavior.NoDrop, standingState, setDrawDepth: true);
        return true;
    }
}

[Serializable, NetSerializable]
public sealed partial class StandingUpDoAfterEvent : SimpleDoAfterEvent;

public enum DropHeldItemsBehavior : byte
{
    NoDrop,
    DropIfStanding,
    AlwaysDrop
}
