using Content.Shared.Silicon.Components;
using Content.Shared.Alert;
using Content.Shared.Bed.Sleep;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Movement.Systems;
using Content.Shared.PowerCell.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicon.Systems;

public sealed class SharedSiliconChargeSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, ComponentInit>(OnSiliconInit);
        SubscribeLocalEvent<SiliconComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);
        SubscribeLocalEvent<SiliconComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<SiliconComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttempt);
        SubscribeLocalEvent<SiliconComponent, ItemSlotEjectAttemptEvent>(OnItemSlotEjectAttempt);
        SubscribeLocalEvent<SiliconComponent, TryingToSleepEvent>(OnTryingToSleep);
    }

    /// <summary>
    ///     Silicon entities can now also be Living player entities. We may want to prevent them from sleeping if they can't sleep.
    /// </summary>
    private void OnTryingToSleep(EntityUid uid, SiliconComponent component, ref TryingToSleepEvent args)
    {
        args.Cancelled = !component.DoSiliconsDreamOfElectricSheep;
    }

    private void OnItemSlotInsertAttempt(EntityUid uid, SiliconComponent component, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled
            || !TryComp<PowerCellSlotComponent>(uid, out var cellSlotComp)
            || !_itemSlots.TryGetSlot(uid, cellSlotComp.CellSlotId, out var cellSlot)
            || cellSlot != args.Slot || args.User != uid)
            return;

        args.Cancelled = true;
    }

    private void OnItemSlotEjectAttempt(EntityUid uid, SiliconComponent component, ref ItemSlotEjectAttemptEvent args)
    {
        if (args.Cancelled
            || !TryComp<PowerCellSlotComponent>(uid, out var cellSlotComp)
            || !_itemSlots.TryGetSlot(uid, cellSlotComp.CellSlotId, out var cellSlot)
            || cellSlot != args.Slot || args.User != uid)
            return;

        args.Cancelled = true;
    }

    private void OnSiliconInit(EntityUid uid, SiliconComponent component, ComponentInit args)
    {
        if (!component.BatteryPowered)
            return;

        _alertsSystem.ShowAlert(uid, AlertType.BorgBattery, component.ChargeState);
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconComponent component, SiliconChargeStateUpdateEvent ev)
    {
        _alertsSystem.ShowAlert(uid, AlertType.BorgBattery, ev.ChargePercent);
    }

    private void OnRefreshMovespeed(EntityUid uid, SiliconComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!component.BatteryPowered)
            return;

        var closest = 0;
        foreach (var state in component.SpeedModifierThresholds)
            if (component.ChargeState >= state.Key && state.Key > closest)
                closest = state.Key;

        var speedMod = component.SpeedModifierThresholds[closest];

        args.ModifySpeed(speedMod, speedMod);
    }
}


public enum SiliconType
{
    Player,
    GhostRole,
    Npc,
}

/// <summary>
///     Event raised when a Silicon's charge state needs to be updated.
/// </summary>
[Serializable, NetSerializable]
public sealed class SiliconChargeStateUpdateEvent : EntityEventArgs
{
    public short ChargePercent { get; }

    public SiliconChargeStateUpdateEvent(short chargePercent)
    {
        ChargePercent = chargePercent;
    }
}
