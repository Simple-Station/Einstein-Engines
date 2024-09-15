using Content.Server.Destructible;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Random;

namespace Content.Server.Teleportation;

public sealed class SquashTeleportSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DestructibleSystem _destructible = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SquashTeleportComponent, StepTriggeredOffEvent>(OnStepTriggeredOff);
    }

    private void OnStepTriggeredOff(Entity<SquashTeleportComponent> entity, ref StepTriggeredOffEvent args) =>
        Teleport(entity, args.Tripper);

    private void Teleport(Entity<SquashTeleportComponent> entity, EntityUid target)
    {
        _destructible.DestroyEntity(entity);

        var coordinates = Transform(entity).Coordinates;
        _xform.SetCoordinates(target, coordinates.Offset(_random.NextVector2(entity.Comp.TeleportRadius)));
        _popup.PopupEntity(Loc.GetString("squash-teleport-teleported-target"), target, target, PopupType.SmallCaution);
    }
}

