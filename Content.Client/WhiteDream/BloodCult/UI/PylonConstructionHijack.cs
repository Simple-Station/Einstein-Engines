using System.Linq;
using Content.Client.Construction;
using Content.Client.Popups;
using Content.Shared.Construction.Prototypes;
using Content.Shared.WhiteDream.BloodCult.Components;
using Robust.Client.Placement;
using Robust.Client.Utility;
using Robust.Shared.Map;

namespace Content.Client.WhiteDream.BloodCult.UI;

public sealed class PylonConstructionHijack(
    ConstructionPrototype? prototype,
    IEntityManager entityManager,
    EntityUid player)
    : PlacementHijack
{
    private readonly ConstructionSystem _construction = entityManager.System<ConstructionSystem>();
    private readonly EntityLookupSystem _lookup = entityManager.System<EntityLookupSystem>();
    private readonly PopupSystem _popup = entityManager.System<PopupSystem>();

    private const float PylonLookupRange = 10;

    /// <inheritdoc />
    public override bool CanRotate => false;

    /// <inheritdoc />
    public override bool HijackPlacementRequest(EntityCoordinates coordinates)
    {
        if (prototype is null)
            return true;

        if (CheckForOtherPylons(coordinates, PylonLookupRange))
        {
            _popup.PopupClient(Loc.GetString("pylon-placement-another-pylon-nearby"), player, player);
            return true;
        }

        _construction.ClearAllGhosts();
        var dir = Manager.Direction;
        _construction.SpawnGhost(prototype, coordinates, dir);
        return true;
    }

    /// <inheritdoc />
    public override bool HijackDeletion(EntityUid entity)
    {
        if (IoCManager.Resolve<IEntityManager>().HasComponent<ConstructionGhostComponent>(entity))
        {
            _construction.ClearGhost(entity.GetHashCode());
        }

        return true;
    }

    /// <inheritdoc />
    public override void StartHijack(PlacementManager manager)
    {
        base.StartHijack(manager);
        manager.CurrentTextures = prototype?.Layers.Select(sprite => sprite.DirFrame0()).ToList();
    }

    private bool CheckForOtherPylons(EntityCoordinates coordinates, float range)
    {
        var entities = _lookup.GetEntitiesInRange(coordinates, range);
        return entities.Any(entityManager.HasComponent<PylonComponent>);
    }
}
