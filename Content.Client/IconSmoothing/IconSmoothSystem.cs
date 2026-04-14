using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Shared.IconSmoothing;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Map.Enumerators;
using static Robust.Client.GameObjects.SpriteComponent;

namespace Content.Client.IconSmoothing
{
    // TODO: just make this set appearance data?
    /// <summary>
    ///     Entity system implementing the logic for <see cref="IconSmoothComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed partial class IconSmoothSystem : EntitySystem
    {
        [Dependency] private readonly SpriteSystem _sprite = default!;
        [Dependency] private readonly MapSystem _map = default!;

        private readonly HashSet<EntityUid> _dirtyEntities = new();
        private readonly HashSet<EntityUid> _anchorChangedEntities = new();

        private EntityQuery<IconSmoothComponent> _smoothQuery;
        private EntityQuery<SpriteComponent> _spriteQuery;
        private EntityQuery<TransformComponent> _xformQuery;

        public void SetEnabled(EntityUid uid, bool value, IconSmoothComponent? component = null)
        {
            if (!Resolve(uid, ref component, false) || value == component.Enabled)
                return;

            component.Enabled = value;
            DirtyNeighbours(uid, component);
        }

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<IconSmoothComponent, AnchorStateChangedEvent>(OnAnchorChanged);
            SubscribeLocalEvent<IconSmoothComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<IconSmoothComponent, ComponentStartup>(OnStartup);

            _spriteQuery = GetEntityQuery<SpriteComponent>();
            _smoothQuery = GetEntityQuery<IconSmoothComponent>();
            _xformQuery = GetEntityQuery<TransformComponent>();
        }
        private static readonly (EdgeLayer, Vector2)[] EdgeLayerOffsets =
        {
            (EdgeLayer.South, new Vector2(0, -1f)),
            (EdgeLayer.East, new Vector2(1f, 0f)),
            (EdgeLayer.North, new Vector2(0, 1f)),
            (EdgeLayer.West, new Vector2(-1f, 0f))
        };
        private void OnStartup(EntityUid uid, IconSmoothComponent component, ComponentStartup args)
        {
            var xform = Transform(uid);
            var sprite = Comp<SpriteComponent>(uid);

            if (xform.Anchored)
            {
                if (TryComp<MapGridComponent>(xform.GridUid, out var grid))
                    component.LastPosition = (xform.GridUid.Value, grid.TileIndicesFor(xform.Coordinates));
                else
                    component.LastPosition = (null, new Vector2i(0, 0));

                DirtyNeighbours(uid, component);
            }

            // set up edge layers, if present
            foreach (var (edgeLayerKey, edgeLayerOffset) in EdgeLayerOffsets)
            {
                if(!_sprite.LayerMapTryGet((uid, sprite), edgeLayerKey, out var edgeLayerIndex, false))
                    continue;
                _sprite.LayerSetOffset((uid, sprite), edgeLayerIndex, edgeLayerOffset);
                _sprite.LayerSetVisible((uid, sprite), edgeLayerIndex, false);
            }

            if (component.Mode != IconSmoothingMode.Corners)
                return;

            SetCornerLayers(sprite, component);

            if (component.Shader != null)
            {
                sprite.LayerSetShader(CornerLayers.SE, component.Shader);
                sprite.LayerSetShader(CornerLayers.NE, component.Shader);
                sprite.LayerSetShader(CornerLayers.NW, component.Shader);
                sprite.LayerSetShader(CornerLayers.SW, component.Shader);
            }
        }

        public void SetStateBase(EntityUid uid, IconSmoothComponent component, string newState)
        {
            if (!TryComp<SpriteComponent>(uid, out var sprite))
                return;

            component.StateBase = newState;
            SetCornerLayers(sprite, component);
        }

        private void SetCornerLayers(SpriteComponent sprite, IconSmoothComponent component)
        {
            sprite.LayerMapRemove(CornerLayers.SE);
            sprite.LayerMapRemove(CornerLayers.NE);
            sprite.LayerMapRemove(CornerLayers.NW);
            sprite.LayerMapRemove(CornerLayers.SW);

            var state0 = $"{component.StateBase}0";
            sprite.LayerMapSet(CornerLayers.SE, sprite.AddLayerState(state0));
            sprite.LayerSetDirOffset(CornerLayers.SE, DirectionOffset.None);
            sprite.LayerMapSet(CornerLayers.NE, sprite.AddLayerState(state0));
            sprite.LayerSetDirOffset(CornerLayers.NE, DirectionOffset.CounterClockwise);
            sprite.LayerMapSet(CornerLayers.NW, sprite.AddLayerState(state0));
            sprite.LayerSetDirOffset(CornerLayers.NW, DirectionOffset.Flip);
            sprite.LayerMapSet(CornerLayers.SW, sprite.AddLayerState(state0));
            sprite.LayerSetDirOffset(CornerLayers.SW, DirectionOffset.Clockwise);
        }

        private void OnShutdown(EntityUid uid, IconSmoothComponent component, ComponentShutdown args)
        {
            DirtyNeighbours(uid, component);
        }

        public override void FrameUpdate(float frameTime)
        {
            base.FrameUpdate(frameTime);

            // first process anchor state changes.
            foreach (var uid in _anchorChangedEntities)
            {
                if (!_xformQuery.TryGetComponent(uid, out var xform))
                    continue;

                if (xform.MapID == MapId.Nullspace)
                {
                    // in null-space. Almost certainly because it left PVS. If something ever gets sent to null-space
                    // for reasons other than this (or entity deletion), then maybe we still need to update ex-neighbor
                    // smoothing here.
                    continue;
                }

                DirtyNeighbours(uid, comp: null, xform);
            }
            _anchorChangedEntities.Clear();

            // Next, update actual sprites.
            if (_dirtyEntities.Count == 0)
                return;


            // Performance: This could be spread over multiple updates, or made parallel.
            foreach (var uid in _dirtyEntities)
            {
                CalculateNewSprite(uid);
            }
            _dirtyEntities.Clear();
        }

        public void DirtyNeighbours(EntityUid uid, IconSmoothComponent? comp = null, TransformComponent? transform = null)
        {
            if (!_smoothQuery.Resolve(uid, ref comp) || !comp.Running)
                return;

            _dirtyEntities.Add(uid);

            if (!Resolve(uid, ref transform))
                return;

            Vector2i pos;

            if (transform.Anchored && TryComp<MapGridComponent>(transform.GridUid, out var grid))
            {
                pos = grid.CoordinatesToTile(transform.Coordinates);
            }
            else
            {
                // Entity is no longer valid, update around the last position it was at.
                if (comp.LastPosition is not (EntityUid gridId, Vector2i oldPos))
                    return;

                if (!TryComp(gridId, out grid))
                    return;

                pos = oldPos;
            }

            // Yes, we updates ALL smoothing entities surrounding us even if they would never smooth with us.
            DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(1, 0)));
            DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(-1, 0)));
            DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(0, 1)));
            DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(0, -1)));

            if (comp.Mode is IconSmoothingMode.Corners or IconSmoothingMode.NoSprite or IconSmoothingMode.Diagonal)
            {
                DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(1, 1)));
                DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(-1, -1)));
                DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(-1, 1)));
                DirtyEntities(grid.GetAnchoredEntitiesEnumerator(pos + new Vector2i(1, -1)));
            }
        }

        private void DirtyEntities(AnchoredEntitiesEnumerator entities)
        {
            // Instead of doing HasComp -> Enqueue -> TryGetComp, we will just enqueue all entities. Generally when
            // dealing with walls neighboring anchored entities will also be walls, and in those instances that will
            // require one less component fetch/check.
            while (entities.MoveNext(out var entity))
            {
                _dirtyEntities.Add(entity.Value);
            }
        }

        private void OnAnchorChanged(EntityUid uid, IconSmoothComponent component, ref AnchorStateChangedEvent args)
        {
            if (!args.Detaching)
                _anchorChangedEntities.Add(uid);
        }

        private void CalculateNewSprite(EntityUid uid, IconSmoothComponent? smooth = null)
        {
            var xform = Transform(uid);
            MapGridComponent? grid = null;

            if (!_smoothQuery.Resolve(uid, ref smooth, false))
                return;

            if (!_spriteQuery.TryGetComponent(uid, out var sprite))
            {
                Log.Error($"Encountered a icon-smoothing entity without a sprite: {ToPrettyString(uid)}");
                RemCompDeferred(uid, smooth);
                return;
            }

            if (smooth.Mode == IconSmoothingMode.NoSprite
                || !smooth.Enabled
                || !smooth.Running)
            {
                if (!smooth.SmoothEdges)
                    return;

                CalculateEdge(uid, sprite, smooth);
                return;
            }


            Entity<MapGridComponent>? gridEntity = null;
            if (xform.Anchored)
            {
                if (!TryComp(xform.GridUid, out grid))
                {
                    Log.Error($"Failed to calculate IconSmoothComponent sprite in {uid} because grid {xform.GridUid} was missing.");
                    return;
                }
                gridEntity = (xform.GridUid.Value, grid);
            }

            // Instead of an explicit check and/or separate handling, the current code overloads the gridEntity variable
            // (previously "MapGridComponent? grid") to also signify if the entity is unanchored, (by setting it to null)
            // in which case it is handled separately.
            // This exists only because of corners smoothing mode, which doesn't have a "default" icon state per se, but
            // rather 4 default states for respective corners.
            // These methods are also responsible for calling CalculateEdge/UpdateEdge, which is questionable.
            // All of this sucks and should be changed.
            switch (smooth.Mode)
            {
                case IconSmoothingMode.Corners:
                    CalculateNewSpriteCorners((uid, smooth, sprite), gridEntity, xform);
                    break;
                case IconSmoothingMode.CardinalFlags:
                    CalculateNewSpriteCardinal((uid, smooth, sprite), gridEntity, xform);
                    break;
                case IconSmoothingMode.Diagonal:
                    CalculateNewSpriteDiagonal((uid, smooth, sprite), gridEntity, xform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CalculateNewSpriteDiagonal(Entity<IconSmoothComponent, SpriteComponent> entity, MapGridComponent? grid, TransformComponent xform)
        {
            var smooth = entity.Comp1;
            var sprite = entity.Comp2;

            if (grid == null)
            {
                sprite.LayerSetState(0, $"{smooth.StateBase}0");
                return;
            }
            var neighbors = new Direction[]
            {
                Direction.East,
                Direction.SouthEast,
                Direction.South,
            };

            var pos = grid.TileIndicesFor(xform.Coordinates);
            var rotation = xform.LocalRotation;
            var matching = true;

            for (var i = 0; i < neighbors.Length; i++)
            {
                matching &= MatchingEntity(smooth, grid, pos, neighbors[i], rotation);
                if (!matching)
                    break;
            }

            if (matching)
                sprite.LayerSetState(0, $"{smooth.StateBase}1");
            else
                sprite.LayerSetState(0, $"{smooth.StateBase}0");
        }

        private void CalculateNewSpriteCardinal(Entity<IconSmoothComponent, SpriteComponent> entity, MapGridComponent? grid, TransformComponent xform)
        {
            var dirs = CardinalConnectDirs.None;
            var smooth = entity.Comp1;
            var sprite = entity.Comp2;

            if (grid == null)
            {
                sprite.LayerSetState(0, $"{smooth.StateBase}{(int) dirs}");
                return;
            }

            var pos = grid.TileIndicesFor(xform.Coordinates);
            if (MatchingEntity(smooth, grid, pos, Direction.North))
                dirs |= CardinalConnectDirs.North;
            if (MatchingEntity(smooth, grid, pos, Direction.South))
                dirs |= CardinalConnectDirs.South;
            if (MatchingEntity(smooth, grid, pos, Direction.East))
                dirs |= CardinalConnectDirs.East;
            if (MatchingEntity(smooth, grid, pos, Direction.West))
                dirs |= CardinalConnectDirs.West;

            sprite.LayerSetState(0, $"{smooth.StateBase}{(int) dirs}");

            var directions = DirectionFlag.None;

            if ((dirs & CardinalConnectDirs.South) != 0x0)
                directions |= DirectionFlag.South;
            if ((dirs & CardinalConnectDirs.East) != 0x0)
                directions |= DirectionFlag.East;
            if ((dirs & CardinalConnectDirs.North) != 0x0)
                directions |= DirectionFlag.North;
            if ((dirs & CardinalConnectDirs.West) != 0x0)
                directions |= DirectionFlag.West;

            UpdateEdge(entity, directions, sprite);
        }

        private void CalculateNewSpriteCorners(Entity<IconSmoothComponent, SpriteComponent> entity, MapGridComponent? grid, TransformComponent xform)
        {
            var smooth = entity.Comp1;
            var sprite = entity.Comp2;

            var (cornerNE, cornerNW, cornerSW, cornerSE) = grid == null
                ? (CornerFill.None, CornerFill.None, CornerFill.None, CornerFill.None)
                : CalculateCornerFill(grid, smooth, xform);

            // TODO figure out a better way to set multiple sprite layers.
            // This will currently re-calculate the sprite bounding box 4 times.
            // It will also result in 4-8 sprite update events being raised when it only needs to be 1-2.
            // At the very least each event currently only queues a sprite for updating.
            // Oh god sprite component is a mess.
            sprite.LayerSetState(CornerLayers.NE, $"{smooth.StateBase}{(int) cornerNE}");
            sprite.LayerSetState(CornerLayers.SE, $"{smooth.StateBase}{(int) cornerSE}");
            sprite.LayerSetState(CornerLayers.SW, $"{smooth.StateBase}{(int) cornerSW}");
            sprite.LayerSetState(CornerLayers.NW, $"{smooth.StateBase}{(int) cornerNW}");

            var directions = DirectionFlag.None;

            if ((cornerSE & cornerSW) != CornerFill.None)
                directions |= DirectionFlag.South;

            if ((cornerSE & cornerNE) != CornerFill.None)
                directions |= DirectionFlag.East;

            if ((cornerNE & cornerNW) != CornerFill.None)
                directions |= DirectionFlag.North;

            if ((cornerNW & cornerSW) != CornerFill.None)
                directions |= DirectionFlag.West;

            UpdateEdge(entity, directions, sprite);
        }

        private (CornerFill ne, CornerFill nw, CornerFill sw, CornerFill se) CalculateCornerFill(MapGridComponent grid, IconSmoothComponent smooth, TransformComponent xform)
        {
            var pos = grid.TileIndicesFor(xform.Coordinates);
            var n = MatchingEntity(smooth, grid, pos, Direction.North);
            var ne = MatchingEntity(smooth, grid, pos, Direction.NorthEast);
            var e = MatchingEntity(smooth, grid, pos, Direction.East);
            var se = MatchingEntity(smooth, grid, pos, Direction.SouthEast);
            var s = MatchingEntity(smooth, grid, pos, Direction.South);
            var sw = MatchingEntity(smooth, grid, pos, Direction.SouthWest);
            var w = MatchingEntity(smooth, grid, pos, Direction.West);
            var nw = MatchingEntity(smooth, grid, pos, Direction.NorthWest);

            // ReSharper disable InconsistentNaming
            var cornerNE = CornerFill.None;
            var cornerSE = CornerFill.None;
            var cornerSW = CornerFill.None;
            var cornerNW = CornerFill.None;
            // ReSharper restore InconsistentNaming

            if (n)
            {
                cornerNE |= CornerFill.CounterClockwise;
                cornerNW |= CornerFill.Clockwise;
            }

            if (ne)
            {
                cornerNE |= CornerFill.Diagonal;
            }

            if (e)
            {
                cornerNE |= CornerFill.Clockwise;
                cornerSE |= CornerFill.CounterClockwise;
            }

            if (se)
            {
                cornerSE |= CornerFill.Diagonal;
            }

            if (s)
            {
                cornerSE |= CornerFill.Clockwise;
                cornerSW |= CornerFill.CounterClockwise;
            }

            if (sw)
            {
                cornerSW |= CornerFill.Diagonal;
            }

            if (w)
            {
                cornerSW |= CornerFill.Clockwise;
                cornerNW |= CornerFill.CounterClockwise;
            }

            if (nw)
            {
                cornerNW |= CornerFill.Diagonal;
            }

            // Local is fine as we already know it's parented to the grid (due to the way anchoring works).
            switch (xform.LocalRotation.GetCardinalDir())
            {
                case Direction.North:
                    return (cornerSW, cornerSE, cornerNE, cornerNW);
                case Direction.West:
                    return (cornerSE, cornerNE, cornerNW, cornerSW);
                case Direction.South:
                    return (cornerNE, cornerNW, cornerSW, cornerSE);
                default:
                    return (cornerNW, cornerSW, cornerSE, cornerNE);
            }
        }

        private bool MatchingEntity(IconSmoothComponent smooth, MapGridComponent grid, Vector2i tilePosition, Direction offsetDirection, Angle? entityRotation = null) =>
            MatchingEntity(smooth, grid, tilePosition, offsetDirection, out _, entityRotation);

        private bool MatchingEntity(IconSmoothComponent smooth, MapGridComponent grid, Vector2i tilePosition, Direction offsetDirection, [NotNullWhen(true)] out EntityUid? entity, Angle? entityRotation = null)
        {
            if(entityRotation is Angle rot)
                offsetDirection = rot.RotateDir(offsetDirection);

            return MatchingEntity(smooth, _map.GetAnchoredEntitiesEnumerator(grid.Owner, grid, tilePosition.Offset(offsetDirection)), out entity);

        }
        private bool MatchingEntity(IconSmoothComponent smooth, AnchoredEntitiesEnumerator candidates) => MatchingEntity(smooth, candidates, out _);
        private bool MatchingEntity(IconSmoothComponent smooth, AnchoredEntitiesEnumerator candidates, [NotNullWhen(true)] out EntityUid? entity)
        {
            while (candidates.MoveNext(out entity)) // MoveNext() assigns null at its last call
            {
                if (_smoothQuery.TryGetComponent(entity, out var other) &&
                    other.SmoothKey == smooth.SmoothKey &&
                    other.Enabled)
                {
                    return true;
                }
            }
            return false;
        }

        // TODO consider changing this to use DirectionFlags?
        // would require re-labelling all the RSI states.
        [Flags]
        private enum CardinalConnectDirs : byte
        {
            None = 0,
            North = 1,
            South = 2,
            East = 4,
            West = 8
        }


        [Flags]
        private enum CornerFill : byte
        {
            // These values are pulled from Baystation12.
            // I'm too lazy to convert the state names.
            None = 0,

            // The cardinal tile counter-clockwise of this corner is filled.
            CounterClockwise = 1,

            // The diagonal tile in the direction of this corner.
            Diagonal = 2,

            // The cardinal tile clockwise of this corner is filled.
            Clockwise = 4,
        }

        private enum CornerLayers : byte
        {
            SE,
            NE,
            NW,
            SW,
        }
    }
}
