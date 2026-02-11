using Content.Shared.Input;
using Content.Shared.MouseRotator;
using Content.Shared.Movement.Components;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.CombatMode;

public abstract class SharedCombatModeSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    // [Dependency] private   readonly INetManager _netMan = default!;
    // [Dependency] private   readonly SharedActionsSystem _actionsSystem = default!;
    // [Dependency] private   readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // SubscribeLocalEvent<CombatModeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CombatModeComponent, ComponentShutdown>(OnShutdown);
        // SubscribeLocalEvent<CombatModeComponent, ToggleCombatActionEvent>(OnActionPerform);
        SubscribeAllEvent<ToggleCombatModeRequestEvent>(OnToggleCombatModeRequest);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ToggleCombatMode, InputCmdHandler.FromDelegate(ToggleCombatMode, handle: false, outsidePrediction: false))
            .Register<SharedCombatModeSystem>();
    }

    // private void OnMapInit(EntityUid uid, CombatModeComponent component, MapInitEvent args)
    // {
    //     _actionsSystem.AddAction(uid, ref component.CombatToggleActionEntity, component.CombatToggleAction);
    //     Dirty(uid, component);
    // }

    protected virtual void OnShutdown(EntityUid uid, CombatModeComponent component, ComponentShutdown args)
    {
        // _actionsSystem.RemoveAction(uid, component.CombatToggleActionEntity);

        SetMouseRotatorComponents(uid, false);
    }

    // private void OnActionPerform(EntityUid uid, CombatModeComponent component, ToggleCombatActionEvent args)
    // {
    //     if (args.Handled)
    //         return;

    //     args.Handled = true;
    //     SetInCombatMode(uid, !component.IsInCombatMode, component);

    //     // TODO better handling of predicted pop-ups.
    //     // This probably breaks if the client has prediction disabled.

    //     if (!_netMan.IsClient || !Timing.IsFirstTimePredicted)
    //         return;

    //     var msg = component.IsInCombatMode ? "action-popup-combat-enabled" : "action-popup-combat-disabled";
    //     _popup.PopupEntity(Loc.GetString(msg), args.Performer, args.Performer);
    // }

    private void OnToggleCombatModeRequest(ToggleCombatModeRequestEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not {Valid: true, } attached
            || !TryComp<CombatModeComponent>(attached, out var combatMode))
        {
            Log.Warning($"User {args.SenderSession.Name} sent an invalid {nameof(ToggleCombatModeRequestEvent)}");
            return;
        }

        SetInCombatMode(attached, !combatMode.IsInCombatMode, combatMode, false);
    }

    private void ToggleCombatMode(ICommonSession? session)
    {
        if (session?.AttachedEntity is not {Valid: true, } attached
            || !TryComp<CombatModeComponent>(attached, out var combatMode))
            return;

        SetInCombatMode(attached, !combatMode.IsInCombatMode, combatMode, false);
    }

    public void SetCanDisarm(EntityUid entity, bool canDisarm, CombatModeComponent? component = null)
    {
        if (!Resolve(entity, ref component))
            return;

        component.CanDisarm = canDisarm;
    }

    public bool IsInCombatMode(EntityUid? entity, CombatModeComponent? component = null)
    {
        return entity != null && Resolve(entity.Value, ref component, false) && component.IsInCombatMode;
    }

    public virtual void SetInCombatMode(EntityUid entity, bool value, CombatModeComponent? component = null, bool silent = true)
    {
        if (!Resolve(entity, ref component))
            return;

        if (component.IsInCombatMode == value)
            return;

        if (!component.Enable && value)
            return;

        component.IsInCombatMode = value;
        Dirty(entity, component);

        RaiseLocalEvent(entity, new CombatModeToggledEvent(value));

        // if (component.CombatToggleActionEntity != null)
        //     _actionsSystem.SetToggled(component.CombatToggleActionEntity, component.IsInCombatMode);

        // Change mouse rotator comps if flag is set
        if (!component.ToggleMouseRotator || IsNpc(entity))
            return;

        SetMouseRotatorComponents(entity, value);
    }

    public void SetEnable(EntityUid entity, CombatModeComponent component, bool enable)
    {
        component.Enable = enable;
        Dirty(entity, component);
    }

    public void SetMouseRotatorComponents(EntityUid uid, bool value)
    {
        if (value)
        {
            EnsureComp<MouseRotatorComponent>(uid);
            EnsureComp<NoRotateOnMoveComponent>(uid);
        }
        else
        {
            RemComp<MouseRotatorComponent>(uid);
            RemComp<NoRotateOnMoveComponent>(uid);
        }
    }

    // todo: When we stop making fucking garbage abstract shared components, remove this shit too.
    protected abstract bool IsNpc(EntityUid uid);
}

public sealed class CombatModeToggledEvent(bool combatMode) : EntityEventArgs
{
    public bool CombatMode = combatMode;
}

[Serializable, NetSerializable]
public sealed class ToggleCombatModeRequestEvent : EntityEventArgs;
