using Content.Shared.Containers.ItemSlots;
using Content.Shared.Coordinates;
using Robust.Shared.Audio;
using Content.Shared.Audio;
using Robust.Shared.Network;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Content.Shared.Popups;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Robust.Shared.Timing;

namespace Content.Shared._Arcadis.Computer;

public sealed class ModularComputerSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    [Dependency] private readonly INetManager _netMan = default!;

    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public string BlankDiskPrototype = "UnburnedDiskPrototype";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModularComputerComponent, EntInsertedIntoContainerMessage>(InsertDisk);
        SubscribeLocalEvent<ModularComputerComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<ModularComputerComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }

    private void OnExamined(EntityUid uid, ModularComputerComponent component, ExaminedEvent args)
    {
        if (!TryComp(uid, out ItemSlotsComponent? slots)
            || !_itemSlots.TryGetSlot(uid, component.DiskSlot, out var diskSlot, slots))
            return;

        if (diskSlot.Item == null || !TryComp(diskSlot.Item, out ComputerDiskComponent? diskComp))
        {
            args.PushMarkup(Loc.GetString("modular-computer-examine-no-disk"));
            return;
        }

        if (diskComp.ProgramPrototypeEntity == null)
        {
            args.PushMarkup(Loc.GetString("modular-computer-examine-disk-error"));
            return;
        }

        args.PushMarkup(Loc.GetString("modular-computer-examine-has-program", ("program", EntityManager.GetComponent<MetaDataComponent>(diskComp.ProgramPrototypeEntity.Value).EntityName)));
    }
    private void OnActivate(EntityUid uid, ModularComputerComponent component, ActivateInWorldEvent args)
    {
        // go figure it out yourself
        if (!TryComp(uid, out ItemSlotsComponent? slots)
            || !_itemSlots.TryGetSlot(uid, component.DiskSlot, out var diskSlot, slots))
            return;

        if (diskSlot.Item == null || !TryComp(diskSlot.Item, out ComputerDiskComponent? diskComp))
        {
            _popupSystem.PopupPredicted(Loc.GetString("modular-computer-no-program"), uid, args.User);
            return;
        }

        if (diskComp.ProgramPrototypeEntity == null)
        {
            _popupSystem.PopupPredicted(Loc.GetString("modular-computer-no-program-on-disk"), uid, args.User);
            return;
        }

        if (_gameTiming.IsFirstTimePredicted || _netMan.IsServer) {
            var activateMsg = new ActivateInWorldEvent(args.User, diskComp.ProgramPrototypeEntity.Value, true);
            RaiseLocalEvent(diskComp.ProgramPrototypeEntity.Value, activateMsg);
        }
    }

    private void InsertDisk(EntityUid uid, ModularComputerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.DiskSlot
            || !TryComp(uid, out ItemSlotsComponent? slots)
            || !_itemSlots.TryGetSlot(uid, component.DiskSlot, out var diskSlot, slots)
            || diskSlot.Item is null
            || !TryComp(diskSlot.Item, out ComputerDiskComponent? diskComp))
            return;

        UpdateComputer((uid, component), diskComp, diskSlot);

        if (diskComp.ProgramPrototypeEntity is null
            || _netMan.IsClient)
            return;

        _audioSystem.PlayPvs(component.DiskInsertSound, uid, AudioParams.Default.WithVolume(+4f));
    }


    private void UpdateComputer(Entity<ModularComputerComponent> computer, ComputerDiskComponent diskComp, ItemSlot diskSlot)
    {
        if (diskSlot.Item is null
            || diskComp.ProgramPrototype == BlankDiskPrototype)
            return;

        EntityUid magicComputerEntity;

        if (diskComp.ProgramPrototypeEntity == null || diskComp.PersistState != true)
        {
            if (diskComp.ProgramPrototypeEntity != null)
                QueueDel(diskComp.ProgramPrototypeEntity.Value);

            magicComputerEntity = Spawn(diskComp.ProgramPrototype, computer.Owner.ToCoordinates());
            diskComp.ProgramPrototypeEntity = magicComputerEntity;
        }
        else
            magicComputerEntity = diskComp.ProgramPrototypeEntity.Value;

        _transform.SetParent(magicComputerEntity, diskSlot.Item.Value);
    }
}
