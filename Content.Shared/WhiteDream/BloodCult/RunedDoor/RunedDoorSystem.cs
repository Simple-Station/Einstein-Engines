using Content.Shared.Doors;
using Content.Shared.Prying.Components;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Constructs;

namespace Content.Shared.WhiteDream.BloodCult.RunedDoor;

public sealed class RunedDoorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RunedDoorComponent, BeforeDoorOpenedEvent>(OnBeforeDoorOpened);
        SubscribeLocalEvent<RunedDoorComponent, BeforeDoorClosedEvent>(OnBeforeDoorClosed);
        SubscribeLocalEvent<RunedDoorComponent, BeforePryEvent>(OnBeforePry);
    }

    private void OnBeforeDoorOpened(Entity<RunedDoorComponent> door, ref BeforeDoorOpenedEvent args)
    {
        if (args.User is not { } user)
            return;

        if (!CanInteract(user))
        {
            args.Cancel();
        }
    }

    private void OnBeforeDoorClosed(Entity<RunedDoorComponent> door, ref BeforeDoorClosedEvent args)
    {
        if (args.User is not { } user)
            return;

        if (!CanInteract(user))
        {
            args.Cancel();
        }
    }

    private void OnBeforePry(Entity<RunedDoorComponent> ent, ref BeforePryEvent args)
    {
        args.Cancelled = true;
    }

    private bool CanInteract(EntityUid user)
    {
        return HasComp<BloodCultistComponent>(user) || HasComp<ConstructComponent>(user);
    }
}
