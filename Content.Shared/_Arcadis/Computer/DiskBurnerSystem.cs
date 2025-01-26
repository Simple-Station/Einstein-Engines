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
using Content.Shared.Construction.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._Arcadis.Computer;

public sealed class DiskBurnerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiskBurnerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DiskBurnerComponent, GetVerbsEvent<Verb>>(GetVerb);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }

    private void GetVerb(EntityUid uid, DiskBurnerComponent component, GetVerbsEvent<Verb> args)
    {
        args.Verbs.Add(new Verb
        {
            Act = () => BurnDisk(args.User, uid, component),
            Text = Loc.GetString(component.VerbName),
            // TODO VERB ICON find a better icon
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
        });
    }

    private void BurnDisk(EntityUid user, EntityUid entity, DiskBurnerComponent component)
    {
        if (!TryComp(entity, out ItemSlotsComponent? slots)
            || !_itemSlots.TryGetSlot(entity, component.DiskSlot, out var diskSlot)
            || !_itemSlots.TryGetSlot(entity, component.BoardSlot, out var boardSlot)
            || diskSlot.Item is null
            || boardSlot.Item is null
            || !TryComp(boardSlot.Item.Value, out ComputerBoardComponent? boardComp)
            || boardComp.ModularComputerProgramPrototype is null
            || !TryComp(diskSlot.Item.Value, out ComputerDiskComponent? diskComp))
        {
            _popupSystem.PopupPredicted(Loc.GetString("disk-burner-activate-not-ready"), entity, user);
            return;
        }

        diskComp.ProgramPrototype = boardComp.ModularComputerProgramPrototype.Value;
        _popupSystem.PopupPredicted(Loc.GetString("disk-burner-activate-finished"), entity, user);

    }

    private void OnExamined(EntityUid uid, DiskBurnerComponent component, ExaminedEvent args)
    {
        if (!TryComp(uid, out ItemSlotsComponent? slots)
            || !_itemSlots.TryGetSlot(uid, component.DiskSlot, out var diskSlot)
            || !_itemSlots.TryGetSlot(uid, component.BoardSlot, out var boardSlot))
        {
            args.PushMarkup(Loc.GetString("disk-burner-admemes-fail"));
            return;
        }

        if (diskSlot.Item is null || boardSlot.Item is null)
        {
            var missing = new List<string>();

            if (diskSlot.Item is null)
                missing.Add("disk");

            if (boardSlot.Item is null)
                missing.Add("board");

            args.PushMarkup(Loc.GetString("disk-burner-missing", ("missing", string.Join(", or ", missing))));
            return;
        }

        if (!TryComp(diskSlot.Item.Value, out ComputerDiskComponent? diskComp))
        {
            args.PushMarkup(Loc.GetString("disk-burner-bad-disk"));
            return;
        }

        if (!TryComp(boardSlot.Item.Value, out ComputerBoardComponent? boardComp))
        {
            args.PushMarkup(Loc.GetString("disk-burner-incompatible-board"));
            return;
        }

        if (boardComp.ModularComputerProgramPrototype is null)
        {
            args.PushMarkup(Loc.GetString("disk-burner-incompatible-board"));
            return;
        }

        args.PushMarkup(Loc.GetString("disk-burner-ready"));

    }
}
