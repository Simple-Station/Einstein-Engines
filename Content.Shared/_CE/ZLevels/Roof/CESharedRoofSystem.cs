/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._CE.ZLevels.Roof;

/// <summary>
/// Systems that automatically covers tiles with roofs (or removes roofs)
/// if there is a tile on one of the levels above in the ZLevels network.
/// </summary>
public abstract class CESharedRoofSystem : EntitySystem
{
    [Dependency] protected readonly CESharedZLevelsSystem ZLevel = default!;
    [Dependency] protected readonly SharedRoofSystem Roof = default!;
    [Dependency] protected readonly SharedMapSystem Map = default!;
    [Dependency] protected readonly ITileDefinitionManager TilDefMan = default!;

    protected EntityQuery<MapGridComponent> GridQuery;
    protected EntityQuery<RoofComponent> RoofQuery;

    public override void Initialize()
    {
        base.Initialize();

        GridQuery = GetEntityQuery<MapGridComponent>();
        RoofQuery = GetEntityQuery<RoofComponent>();

        SubscribeLocalEvent<CEZLevelMapRoofComponent, TileChangedEvent>(OnTileChanged);
    }

    /// <summary>
    /// When changing tiles, we iteratively go down to the end of the ZLevels network, repeatedly calculating whether the tiles at the bottom now have a roof or not.
    /// </summary>
    private void OnTileChanged(Entity<CEZLevelMapRoofComponent> ent, ref TileChangedEvent args)
    {
        if (!GridQuery.TryComp(ent, out var currentMapGrid))
            return;
        if (!RoofQuery.TryComp(ent, out var currentRoof))
            return;
        if (!TryComp<CEZLevelMapComponent>(ent, out var zLevelMapComp))
            return;

        if (args.Changes.Length == 0)
            return;

        Dictionary<Vector2i, bool> roofMap = new();
        foreach (var change in args.Changes)
        {
            var tileDef = (ContentTileDefinition)TilDefMan[change.NewTile.TypeId];

            var roovedAbove = Roof.IsRooved((ent, currentMapGrid, currentRoof), change.GridIndices);
            var roovedTile = !tileDef.Transparent;
            roofMap.Add(change.GridIndices, roovedAbove || roovedTile);
        }

        var mapsBelow = ZLevel.GetAllMapsBelow((ent, zLevelMapComp));

        if (mapsBelow.Count == 0)
            return;

        foreach (var mapBelow in mapsBelow)
        {
            if (!GridQuery.TryComp(mapBelow, out var mapGridBelow))
                continue;

            var roofBelow = EnsureComp<RoofComponent>(mapBelow);

            foreach (var (indices, rooved) in roofMap)
            {
                Roof.SetRoof((mapBelow, mapGridBelow, roofBelow), indices, rooved);

                if (Map.TryGetTile(mapGridBelow, indices, out var tile) && !tile.IsEmpty)
                    roofMap[indices] = true;
            }
        }
    }
}
