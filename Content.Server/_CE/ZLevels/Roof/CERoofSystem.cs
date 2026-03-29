/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using Content.Server._CE.ZLevels.Core;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Roof;
using Content.Shared.Light.Components;
using Content.Shared.Maps;

namespace Content.Server._CE.ZLevels.Roof;

/// <inheritdoc/>
public sealed class CERoofSystem : CESharedRoofSystem
{
    private readonly HashSet<Vector2i> _roofMap = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CEZLevelsNetworkComponent, CEZLevelNetworkUpdatedEvent>(OnNetworkUpdated);
    }

    private void OnNetworkUpdated(Entity<CEZLevelsNetworkComponent> ent, ref CEZLevelNetworkUpdatedEvent args)
    {
        RecalculateNetworkRoofs(ent);
    }

    public void RecalculateNetworkRoofs(Entity<CEZLevelsNetworkComponent> network)
    {
        _roofMap.Clear();

        List<EntityUid> sortedMaps = new();
        foreach (var mapUid in network.Comp.ZLevels
                     .OrderByDescending(kv => kv.Key) // depth sorting
                     .Select(kv => kv.Value)
                     .Where(uid => uid.HasValue)
                     .Select(uid => uid!.Value))
        {
            sortedMaps.Add(mapUid);
        }

        foreach (var map in sortedMaps)
        {
            if (!GridQuery.TryComp(map, out var mapGrid))
                continue;

            var enumerator = Map.GetAllTilesEnumerator(map, mapGrid);
            var roofComp = EnsureComp<RoofComponent>(map);

            while (enumerator.MoveNext(out var tileRef))
            {
                Roof.SetRoof((map, mapGrid, roofComp), tileRef.Value.GridIndices, _roofMap.Contains(tileRef.Value.GridIndices));

                var tileDef = (ContentTileDefinition)TilDefMan[tileRef.Value.Tile.TypeId];

                if (!tileDef.Transparent)
                    _roofMap.Add(tileRef.Value.GridIndices);
            }
        }
    }
}
