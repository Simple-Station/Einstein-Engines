using Robust.Shared.Random;
using Content.Shared.Slippery;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Popups;

namespace Content.Server.Botany.Systems;

public sealed class TeleportingTraitSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeleportingTraitComponent, SlipEvent>(Teleport);
    }

    // sets the potency and the radius
    public static void SetPotencyRadius(float seedPotency, TeleportingTraitComponent comp)
    {
        comp.Potency = seedPotency;
        comp.ProduceTeleportRadius = comp.Potency / comp.PotencyDivide;
    }

    // teleports both the produce and the foolish fool who slipped on it to a random postion limited by the radius
    private void Teleport(EntityUid uid, TeleportingTraitComponent comp, ref SlipEvent args)
    {
        var coordinates = Transform(uid).Coordinates;
        _xform.SetCoordinates(uid, coordinates.Offset(_random.NextVector2(comp.ProduceTeleportRadius)));
        _popup.PopupEntity(Loc.GetString("teleporting-trait-component-slipped"), args.Slipped, args.Slipped, PopupType.SmallCaution);
        _xform.SetCoordinates(args.Slipped, coordinates.Offset(_random.NextVector2(comp.ProduceTeleportRadius)));
        VanishProbablity(uid, comp);
    }

    // chance of being deleted and then spawnin the goop
    private void VanishProbablity(EntityUid uid, TeleportingTraitComponent comp)
    {
        if (!_random.Prob(comp.DeletionChance))
            return;
        Solution vanishSolution = new();
        vanishSolution.AddReagent("Slime", comp.Potency / 2);
        _puddle.TrySpillAt(uid, vanishSolution, out _);
        QueueDel(uid);
    }
}

