using System.Numerics;
using Content.Shared.IconSmoothing;
using Robust.Client.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Client.IconSmoothing;

public sealed partial class IconSmoothSystem
{
//    private void OnEdgeShutdown(EntityUid uid, SmoothEdgeComponent component, ComponentShutdown args)
//    {
//        if (!TryComp<SpriteComponent>(uid, out var sprite))
//            return;
//
//        sprite.LayerMapRemove(EdgeLayer.South);
//        sprite.LayerMapRemove(EdgeLayer.East);
//        sprite.LayerMapRemove(EdgeLayer.North);
//        sprite.LayerMapRemove(EdgeLayer.West);
//    }

    private void CalculateEdge(EntityUid uid, SpriteComponent? sprite = null, IconSmoothComponent? smooth = null)
    {
        if (!Resolve(uid, ref sprite, ref smooth, false))
            return;

        var xform = Transform(uid);

        var directions = DirectionFlag.None;
        
        if (xform.GridUid is EntityUid gridUid && TryComp<MapGridComponent>(gridUid, out var grid))
        {
            var pos = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);

            if (MatchingEntity(smooth, _map.GetAnchoredEntitiesEnumerator(gridUid, grid, pos.Offset(Direction.North))))
                directions |= DirectionFlag.North;
            if (MatchingEntity(smooth, _map.GetAnchoredEntitiesEnumerator(gridUid, grid, pos.Offset(Direction.South))))
                directions |= DirectionFlag.South;
            if (MatchingEntity(smooth, _map.GetAnchoredEntitiesEnumerator(gridUid, grid, pos.Offset(Direction.East))))
                directions |= DirectionFlag.East;
            if (MatchingEntity(smooth, _map.GetAnchoredEntitiesEnumerator(gridUid, grid, pos.Offset(Direction.West))))
                directions |= DirectionFlag.West;
        }

        UpdateEdge(uid, directions, sprite, smooth);
    }

    private void UpdateEdge(EntityUid uid, DirectionFlag directions, SpriteComponent? sprite = null, IconSmoothComponent? smooth = null)
    {
        if (!Resolve(uid, ref sprite, ref smooth, false))
            return;

        for (var i = 0; i < 4; i++)
        {
            var dir = (DirectionFlag) Math.Pow(2, i);
            var edge = GetEdge(dir);

            _sprite.LayerSetVisible((uid, sprite), edge, (dir & directions) == 0x0);
        }
    }

    private EdgeLayer GetEdge(DirectionFlag direction)
    {
        switch (direction)
        {
            case DirectionFlag.South:
                return EdgeLayer.South;
            case DirectionFlag.East:
                return EdgeLayer.East;
            case DirectionFlag.North:
                return EdgeLayer.North;
            case DirectionFlag.West:
                return EdgeLayer.West;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private enum EdgeLayer : byte
    {
        South,
        East,
        North,
        West
    }
}
