using Content.Server.Power.Components;
using Content.Server.Emp;
using Content.Server.PowerCell;
using Content.Shared.Examine;
using Content.Shared.Power;
using Content.Shared.PowerCell.Components;
using Content.Shared.Emp;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Storage.Components;
using Robust.Server.Containers;
using Content.Shared.Whitelist;
using Content.Shared.Inventory;
using System.Linq;

namespace Content.Server.Power.EntitySystems;

[UsedImplicitly]
internal sealed class ChargerSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ChargerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ChargerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<ChargerComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<ChargerComponent, EntRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<ChargerComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<ChargerComponent, InsertIntoEntityStorageAttemptEvent>(OnEntityStorageInsertAttempt);
        SubscribeLocalEvent<ChargerComponent, ExaminedEvent>(OnChargerExamine);

        SubscribeLocalEvent<ChargerComponent, ChargerUpdateStatusEvent>(OnUpdateStatus);

        SubscribeLocalEvent<ChargerComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<ChargerComponent, EmpDisabledRemoved>(OnEmpDisabledRemoved);
    }

    private void OnStartup(EntityUid uid, ChargerComponent component, ComponentStartup args)
    {
        UpdateStatus(uid, component);
    }

    private void OnChargerExamine(EntityUid uid, ChargerComponent component, ExaminedEvent args)
    {
        using (args.PushGroup(nameof(ChargerComponent)))
        {
            // rate at which the charger charges
            args.PushMarkup(Loc.GetString("charger-examine", ("color", "yellow"), ("chargeRate", (int) component.ChargeRate)));

            // try to get contents of the charger
            if (!_container.TryGetContainer(uid, component.SlotId, out var container))
                return;

            if (HasComp<PowerCellSlotComponent>(uid))
                return;

            // if charger is empty and not a power cell type charger, add empty message
            // power cells have their own empty message by default, for things like flash lights
            if (container.ContainedEntities.Count == 0)
            {
                args.PushMarkup(Loc.GetString("charger-empty"));
            }
            else
            {
                // add how much each item is charged it
                foreach (var contained in container.ContainedEntities)
                {
                    if (!TryComp<BatteryComponent>(contained, out var battery))
                        continue;

                    var chargePercentage = (battery.CurrentCharge / battery.MaxCharge) * 100;
                    args.PushMarkup(Loc.GetString("charger-content", ("chargePercentage", (int) chargePercentage)));
                }
            }
        }
    }

    private void StartChargingBattery(EntityUid uid, ChargerComponent component, EntityUid target)
    {
        bool charge = true;

        if (HasComp<EmpDisabledComponent>(uid))
            charge = false;
        else
        if (!TryComp<BatteryComponent>(target, out var battery))
            charge = false;
        else
        if (Math.Abs(battery.MaxCharge - battery.CurrentCharge) < 0.01)
            charge = false;

        // wrap functionality in an if statement instead of returning...
        if (charge)
        {
            var charging = EnsureComp<ChargingComponent>(target);
            charging.ChargerUid = uid;
            charging.ChargerComponent = component;
        }

        // ...so the status always updates (for insertin a power cell)
        UpdateStatus(uid, component);
    }

    private void StopChargingBattery(EntityUid uid, ChargerComponent component, EntityUid target)
    {
        if (HasComp<ChargingComponent>(target))
            RemComp<ChargingComponent>(target);
        UpdateStatus(uid, component);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ChargingComponent>();
        while (query.MoveNext(out var uid, out var charging))
        {
            if (!TryComp<ChargerComponent>(charging.ChargerUid, out var chargerComponent))
                continue;

            if (charging.ChargerComponent.Status == CellChargerStatus.Off || charging.ChargerComponent.Status == CellChargerStatus.Empty)
                continue;

            if (HasComp<EmpDisabledComponent>(charging.ChargerUid))
                continue;

            if (!TryComp<BatteryComponent>(uid, out var battery))
                continue;

            TransferPower(charging.ChargerUid, uid, charging.ChargerComponent, frameTime);

            if (battery.CurrentCharge == battery.MaxCharge)
                StopChargingBattery(charging.ChargerUid, charging.ChargerComponent, uid);
        }
    }

    private void OnPowerChanged(EntityUid uid, ChargerComponent component, ref PowerChangedEvent args)
    {
        UpdateStatus(uid, component);
    }

    private void OnInserted(EntityUid uid, ChargerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (!component.Initialized)
            return;

        if (args.Container.ID != component.SlotId)
            return;

        if (!TryGetBatteries(uid, component, out var batteries))
            return;

        foreach (var battery in batteries)
        {
            StartChargingBattery(uid, component, battery);
        }
    }

    private void OnRemoved(EntityUid uid, ChargerComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != component.SlotId)
            return;

        StopChargingBattery(uid, component, args.Entity);
    }

    /// <summary>
    ///     Verify that the entity being inserted is actually rechargeable.
    /// </summary>
    private void OnInsertAttempt(EntityUid uid, ChargerComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (!component.Initialized)
            return;

        if (args.Container.ID != component.SlotId)
            return;

        if (!TryComp<PowerCellSlotComponent>(args.EntityUid, out var cellSlot))
            return;

        if (!cellSlot.FitsInCharger)
            args.Cancel();
    }

    private void OnEntityStorageInsertAttempt(EntityUid uid, ChargerComponent component, ref InsertIntoEntityStorageAttemptEvent args)
    {
        if (!component.Initialized || args.Cancelled)
            return;

        if (!TryComp<PowerCellSlotComponent>(uid, out var cellSlot))
            return;

        if (!cellSlot.FitsInCharger)
            args.Cancelled = true;
    }

    private void OnUpdateStatus(EntityUid uid, ChargerComponent component, ref ChargerUpdateStatusEvent args)
    {
        UpdateStatus(uid, component);
    }

    private void UpdateStatus(EntityUid uid, ChargerComponent component)
    {
        var status = GetStatus(uid, component);
        TryComp(uid, out AppearanceComponent? appearance);

        if (!_container.TryGetContainer(uid, component.SlotId, out var container))
            return;

        _appearance.SetData(uid, CellVisual.Occupied, container.ContainedEntities.Count != 0, appearance);
        if (component.Status == status || !TryComp(uid, out ApcPowerReceiverComponent? receiver))
            return;

        component.Status = status;

        switch (component.Status)
        {
            case CellChargerStatus.Off:
                receiver.Load = 0;
                _appearance.SetData(uid, CellVisual.Light, CellChargerStatus.Off, appearance);
                break;
            case CellChargerStatus.Empty:
                receiver.Load = 0;
                _appearance.SetData(uid, CellVisual.Light, CellChargerStatus.Empty, appearance);
                break;
            case CellChargerStatus.Charging:
                receiver.Load = component.ChargeRate;
                _appearance.SetData(uid, CellVisual.Light, CellChargerStatus.Charging, appearance);
                break;
            case CellChargerStatus.Charged:
                receiver.Load = 0;
                _appearance.SetData(uid, CellVisual.Light, CellChargerStatus.Charged, appearance);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnEmpPulse(EntityUid uid, ChargerComponent component, ref EmpPulseEvent args)
    {
        // we don't care if we haven't been disabled
        if (!args.Disabled)
            return;

        // if the recharger is hit by an emp pulse,
        // stop recharging contained batteries to save resources
        if (!TryGetBatteries(uid, component, out var batteries))
            return;

        foreach (var battery in batteries)
        {
            StopChargingBattery(uid, component, battery);
        }
    }

    private void OnEmpDisabledRemoved(EntityUid uid, ChargerComponent component, ref EmpDisabledRemoved args)
    {
        // if an emp disable subsides,
        // attempt to start charging all batteries
        if (!TryGetBatteries(uid, component, out var batteries))
            return;

        foreach (var battery in batteries)
        {
            StartChargingBattery(uid, component, battery);
        }
    }

    private CellChargerStatus GetStatus(EntityUid uid, ChargerComponent component)
    {
        if (!component.Portable)
        {
            if (!TryComp(uid, out TransformComponent? transformComponent) || !transformComponent.Anchored)
                return CellChargerStatus.Off;
        }

        if (!TryComp(uid, out ApcPowerReceiverComponent? apcPowerReceiverComponent))
            return CellChargerStatus.Off;

        if (!component.Portable && !apcPowerReceiverComponent.Powered)
            return CellChargerStatus.Off;

        if (HasComp<EmpDisabledComponent>(uid))
            return CellChargerStatus.Off;

        if (!_container.TryGetContainer(uid, component.SlotId, out var container))
            return CellChargerStatus.Off;

        if (container.ContainedEntities.Count == 0)
            return CellChargerStatus.Empty;

        var statusOut = CellChargerStatus.Off;

        if (!TryGetBatteries(uid, component, out var batteries))
            return CellChargerStatus.Off;

        foreach (var battery in batteries)
        {
            // if all batteries are either EMP'd or fully charged, represent the charger as fully charged
            statusOut = CellChargerStatus.Charged;

            if (HasComp<EmpDisabledComponent>(battery))
                continue;

            if (!HasComp<ChargingComponent>(battery))
                continue;

            // if we have atleast one battery being charged, represent the charger as charging;
            statusOut = CellChargerStatus.Charging;
            break;
        }
        return statusOut;
    }

    // todo: transferring power responsibility is moved to the charging component and baked rather than iterated
    private void TransferPower(EntityUid uid, EntityUid targetEntity, ChargerComponent component, float frameTime)
    {
        if (!TryComp(uid, out ApcPowerReceiverComponent? receiverComponent))
            return;

        if (!receiverComponent.Powered)
            return;

        if (!TryComp<BatteryComponent>(targetEntity, out var heldBattery))
            return;

        _battery.TrySetCharge(targetEntity, Math.Min(heldBattery.CurrentCharge + component.ChargeRate * frameTime, heldBattery.MaxCharge), heldBattery);

        UpdateStatus(uid, component);
    }

    /*
        breadth first search to prioritise recharging batteries over containers with batteries
        - WarMechanic
    */
    private bool TryGetBatteries(EntityUid uid, ChargerComponent component, [NotNullWhen(true)] out List<EntityUid> batteries)
    {
        batteries = new List<EntityUid>();

        // if we don't have a container to charge batteries, no
        if (!_container.TryGetContainer(uid, component.SlotId, out var container))
        {
            Log.Warning($"Charger at {uid} does not have a corresponding container!");
            return false;
        }

        if (container.ContainedEntities.Count == 0)
        {
            Log.Warning($"Charger at {uid} does not contain entities!");
            return false;
        }

        var searchPq = new PriorityQueue<EntityUid, int>();

        // add all contained entities to search
        container.ContainedEntities.All(x => { searchPq.Enqueue(x, 0); return true; });

        int steps = 0;
        while (searchPq.Count > 0)
        {
            steps++;
            if (SearchStep(uid, component, ref searchPq, out var batteryUid, out var batteryComponent))
            {
                batteries.Add(batteryUid.Value);
            }

            if (batteries.Count >= component.MaxBatteries)
                break;
        }

        if (batteries.Count > 0)
            return true;

        Log.Warning($"Charger at {uid} does not contain batteries!");
        return false;
    }

    private bool TryGetBattery(EntityUid uid, ChargerComponent component, [NotNullWhen(true)] out EntityUid? battery)
    {
        battery = null;
        if (!TryGetBatteries(uid, component, out var batteries))
            return false;

        battery = batteries[0];
        return true;
    }

    private bool SearchStep(EntityUid uid, ChargerComponent component, ref PriorityQueue<EntityUid, int> pq, [NotNullWhen(true)] out EntityUid? batteryUid, [NotNullWhen(true)] out BatteryComponent? batteryComponent)
    {
        batteryUid = null;
        batteryComponent = null;

        if (!pq.TryDequeue(out var searchUid, out int depth))
            return false;

        // try get a battery directly on the inserted entity as long as its permitted in the whitelist
        if (_whitelistSystem.IsWhitelistPassOrNull(component.ChargeWhitelist, searchUid) && TryComp(searchUid, out batteryComponent))
        {
            batteryUid = searchUid;
            return true;
        }

        // if we're not allowed to search this object, cancel
        if (_whitelistSystem.IsWhitelistFail(component.SearchWhitelist, searchUid)
            || !HasComp<ContainerManagerComponent>(searchUid))
            return false;

        // recursively test for containers instead
        var containers = _container.GetAllContainers(searchUid);
        if (containers.Count() == 0)
            return false;

        foreach (var container in containers)
        {
            foreach (EntityUid containedEntity in container.ContainedEntities) { pq.Enqueue(containedEntity, depth + 1); }
        }

        return false;
    }
}

[ByRefEvent]
public record struct ChargerUpdateStatusEvent();

// Goobstation - Modsuits stuff
[ByRefEvent]
public record struct FindInventoryBatteryEvent() : IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;

    public Entity<BatteryComponent>? FoundBattery { get; set; }
}