using System.Collections.Immutable;
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

            // mark us and anyone who could connect wit us dirty, if possible
            if (xform.Anchored && TryComp<MapGridComponent>(xform.GridUid, out var grid))
            {
                component.GridPosition = (xform.GridUid.Value, grid.TileIndicesFor(xform.Coordinates));
                DirtyNeighbours(uid, component);
            }

            // set up edge layers, if present
            var cachedLayers = new List<EdgeLayer>();
            foreach (var (edgeLayerKey, edgeLayerOffset) in EdgeLayerOffsets)
            {
                if(!_sprite.LayerMapTryGet((uid, sprite), edgeLayerKey, out var edgeLayerIndex, false))
                    continue;
                _sprite.LayerSetOffset((uid, sprite), edgeLayerIndex, edgeLayerOffset);
                _sprite.LayerSetVisible((uid, sprite), edgeLayerIndex, false);
                cachedLayers.Add(edgeLayerKey);
            }
            // the edge layesr are not supposed to change afterwards
            // if you *really* have to do something to them,
            // change the relevant types to a regular array.
            component.SmoothEdgeLayers = cachedLayers.ToImmutableArray();

            // corner mode snowflake treatment
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
                if(!TerminatingOrDeleted(uid))
                    CalculateNewSprite(uid);
            }
            _dirtyEntities.Clear();
        }

        private void DirtyNeighbours(EntityUid uid, IconSmoothComponent? comp = null, TransformComponent? transform = null)
        {
            if (!_smoothQuery.Resolve(uid, ref comp) ||
                comp.GridPosition is null)
                return;


            if (!Resolve(uid, ref transform))
                return;

            var (gridId, pos) = comp.GridPosition.Value;

            if (!TryComp<MapGridComponent>(gridId, out var grid))
                return;

            _dirtyEntities.Add(uid);

            // Yes, we updates ALL smoothing entities surrounding us even if they would never smooth with us.
            // // Why?
            DirtyEntities(grid, pos + new Vector2i(1, 0));
            DirtyEntities(grid, pos + new Vector2i(-1, 0));
            DirtyEntities(grid, pos + new Vector2i(0, 1));
            DirtyEntities(grid, pos + new Vector2i(0, -1));

            if (comp.Mode is IconSmoothingMode.Corners or IconSmoothingMode.NoSprite or IconSmoothingMode.Diagonal)
            {
                DirtyEntities(grid, pos + new Vector2i(1, 1));
                DirtyEntities(grid, pos + new Vector2i(-1, -1));
                DirtyEntities(grid, pos + new Vector2i(-1, 1));
                DirtyEntities(grid, pos + new Vector2i(1, -1));
            }
        }

        private void DirtyEntities(MapGridComponent grid, Vector2i tilePos)
        {
            var entities = _map.GetAnchoredEntitiesEnumerator(grid.Owner, grid, tilePos);
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
            if (args.Detaching)
                return;

            var xform = args.Transform;
            if(xform.Anchored && TryComp<MapGridComponent>(xform.GridUid, out var grid))
            {
                component.GridPosition = (xform.GridUid.Value, _map.TileIndicesFor(xform.GridUid.Value, grid, xform.Coordinates));
                DirtyNeighbours(uid, component, xform);
            }
            else
            {
                DirtyNeighbours(uid, component, xform);
                component.GridPosition = null;
            }
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
                CalculateEdge(uid, sprite, smooth);
                return;
            }

            // remains null if entity is not anchored, prompting special behaviour from the methods below 
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

            // these methods are also responsible for calling CalculateEdge/UpdateEdge.
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
                case IconSmoothingMode.Horizontal:
                    CalculateNewSpriteHorizontal((uid, smooth, sprite), gridEntity, xform);
                    break;
                case IconSmoothingMode.Vertical:
                    CalculateNewSpriteVertical((uid, smooth, sprite), gridEntity, xform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private bool MatchingEntity(IconSmoothComponent smooth, MapGridComponent grid, Vector2i tilePosition, Direction offsetDirection, Angle? entityRotation = null, Func<EntityUid, bool>? predicate = null) =>
            MatchingEntity(smooth, grid, tilePosition, offsetDirection, out _, entityRotation, predicate);

        private bool MatchingEntity(IconSmoothComponent smooth, MapGridComponent grid, Vector2i tilePosition, Direction offsetDirection, [NotNullWhen(true)] out EntityUid? entity, Angle? entityRotation = null, Func<EntityUid, bool>? predicate = null)
        {
            if(entityRotation is Angle rot)
                offsetDirection = rot.RotateDir(offsetDirection);

            return MatchingEntity(smooth, _map.GetAnchoredEntitiesEnumerator(grid.Owner, grid, tilePosition.Offset(offsetDirection)), out entity, predicate);

        }
        private bool MatchingEntity(IconSmoothComponent smooth, AnchoredEntitiesEnumerator candidates, Func<EntityUid, bool>? predicate = null) => MatchingEntity(smooth, candidates, out _, predicate);
        private bool MatchingEntity(IconSmoothComponent smooth, AnchoredEntitiesEnumerator candidates, [NotNullWhen(true)] out EntityUid? entity, Func<EntityUid, bool>? predicate = null)
        {
            while (candidates.MoveNext(out entity)) // MoveNext() assigns null at its last call
            {
                if (_smoothQuery.TryGetComponent(entity, out var other) &&
                    other.SmoothKey == smooth.SmoothKey &&
                    other.Enabled)
                {
                    if (predicate is null || predicate(entity.Value))
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
