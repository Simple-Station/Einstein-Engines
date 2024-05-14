using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using Content.Shared.Popups;

namespace Content.Shared.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedPlasmaTransferSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPlasmaVesselSystem _plasmaVessel = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlasmaTransferComponent, ComponentStartup>(OnCompInit);
        SubscribeLocalEvent<PlasmaTransferComponent, ComponentShutdown>(OnCompRemove);
        SubscribeLocalEvent<PlasmaTransferComponent, TransferPlasmaActionEvent>(OnPlasmaTransfer);

    }

    /// <summary>
    /// Giveths the action to preform making acid on the entity
    /// </summary>
    private void OnCompInit(EntityUid uid, PlasmaTransferComponent comp, ComponentStartup args)
    {
        _actions.AddAction(uid, ref comp.ActionEntity, comp.Action);
    }

    public void OnPlasmaTransfer(EntityUid uid, PlasmaTransferComponent component, TransferPlasmaActionEvent args)
    {
        if (!HasComp<PlasmaVesselComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("alien-transfer-fail"), uid);
            return;
        }

        var plasmaVesselSelf = Comp<PlasmaVesselComponent>(uid);
        var plasmaVesselTarget = Comp<PlasmaVesselComponent>(args.Target);
        if (plasmaVesselSelf.Plasma >= component.Amount && component.Amount + plasmaVesselTarget.Plasma < plasmaVesselTarget.PlasmaRegenCap)
        {
            _plasmaVessel.ChangePlasmaAmount(uid, -component.Amount);
            _plasmaVessel.ChangePlasmaAmount(args.Target, component.Amount);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("alien-transfer-fail"), uid);
        }

    }

    /// <summary>
    /// Takeths away the action to preform making acid from the entity.
    /// </summary>
    private void OnCompRemove(EntityUid uid, PlasmaTransferComponent comp, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, comp.ActionEntity);
    }
}
