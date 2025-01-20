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

public sealed class ComputerDiskSystem : EntitySystem
{
    public string BlankDiskPrototype = "UnburnedDiskPrototype";

    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComputerDiskComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }

    private void OnExamined(EntityUid uid, ComputerDiskComponent component, ExaminedEvent args)
    {
        if (component.ProgramPrototype == BlankDiskPrototype)
            args.PushMarkup(Loc.GetString("program-disk-no-program"));
        else
        {
            if (!_protoMan.TryIndex(component.ProgramPrototype, out EntityPrototype? prototype))
                args.PushMarkup(Loc.GetString("program-disk-error"));
            else
                args.PushMarkup(Loc.GetString("program-disk-has-program", ("program", prototype.Name)));
        }
    }
}
