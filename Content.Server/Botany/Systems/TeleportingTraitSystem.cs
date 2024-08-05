using System.Numerics;
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
        var xform = Transform(uid);
        var mapPos = _xform.GetWorldPosition(xform);
        var radius = comp.ProduceTeleportRadius;
        var gridBounds = new Box2(mapPos - new Vector2(radius, radius), mapPos + new Vector2(radius, radius));
        var randomX = _random.NextFloat(gridBounds.Left, gridBounds.Right);
        var randomY = _random.NextFloat(gridBounds.Bottom, gridBounds.Top);
        //probably a better way to do this, but i have no clue at all
        var otherRandomX = _random.NextFloat(gridBounds.Left, gridBounds.Right);
        var otherRandomY = _random.NextFloat(gridBounds.Bottom, gridBounds.Top);
        var producePos = new Vector2(randomX, randomY);
        var otherPos = new Vector2(otherRandomX, otherRandomY);
        _xform.SetWorldPosition(uid, producePos);
        _popup.PopupEntity(Loc.GetString("teleporting-trait-component-slipped"), args.Slipped, args.Slipped, PopupType.SmallCaution);
        _xform.SetWorldPosition(args.Slipped, otherPos);
        VanishProbablity(uid, comp);
    }

    // chance of being deleted and then spawnin the goop
    private void VanishProbablity(EntityUid uid, TeleportingTraitComponent comp)
    {
        if (!_random.Prob(comp.DeletionChance))
            return;
        QueueDel(uid);
        Solution vanishSolution = new();
        vanishSolution.AddReagent("Slime", comp.Potency / 2);
        _puddle.TrySpillAt(uid, vanishSolution, out _);
    }
}

