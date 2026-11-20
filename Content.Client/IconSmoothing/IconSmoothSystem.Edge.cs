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
        
        if (smooth.SmoothEdgeLayers.Length == 0)
            return;

        var xform = Transform(uid);

        var directions = DirectionFlag.None;

        if (xform.GridUid is EntityUid gridUid && TryComp<MapGridComponent>(gridUid, out var grid))
        {
            var pos = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);

            if (MatchingEntity(smooth, grid, pos, Direction.North, xform.LocalRotation))
                directions |= DirectionFlag.North;
            if (MatchingEntity(smooth, grid, pos, Direction.South, xform.LocalRotation))
                directions |= DirectionFlag.South;
            if (MatchingEntity(smooth, grid, pos, Direction.East, xform.LocalRotation))
                directions |= DirectionFlag.East;
            if (MatchingEntity(smooth, grid, pos, Direction.West, xform.LocalRotation))
                directions |= DirectionFlag.West;
        }

        UpdateEdge(uid, directions, sprite, smooth);
    }

    private void UpdateEdge(EntityUid uid, DirectionFlag directions, SpriteComponent? sprite = null, IconSmoothComponent? smooth = null, bool invert = false)
    {
        if (!Resolve(uid, ref sprite, ref smooth, false))
            return;

        if (smooth.SmoothEdgeLayers.Length == 0)
            return;

        foreach (var edge in smooth.SmoothEdgeLayers)
        {
            var dir = GetDir(edge);
            var visible = (dir & directions) == 0x0;

            _sprite.LayerSetVisible((uid, sprite), edge, visible ^ invert);
        }
    }

    private DirectionFlag GetDir(EdgeLayer direction)
    {
        switch (direction)
        {
            case EdgeLayer.South:
                return DirectionFlag.South;
            case EdgeLayer.East:
                return DirectionFlag.East;
            case EdgeLayer.North:
                return DirectionFlag.North;
            case EdgeLayer.West:
                return DirectionFlag.West;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
