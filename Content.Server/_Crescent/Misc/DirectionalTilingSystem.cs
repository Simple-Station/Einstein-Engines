using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Server.Decals;
using Content.Shared.Decals;
using Content.Shared.Entry;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Placement;
using Robust.Shared.Prototypes;


namespace Content.Server._Crescent;


/// <summary>
/// SPCR/MLGTASTICa 2025
/// Handles creating the decals to create the illusion of directional tiles.
/// Based off the System used by the Eris SS13 Codebase.
/// This file is licensed under MIT. Feel free to do whatever.
/// </summary>
///
public sealed class DirectionalTilingSystem : EntitySystem
{
    [Dependency] private readonly TileSystem _tileSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly DecalSystem _decalSystem = default!;
    [Dependency] private readonly ITileDefinitionManager _tiles = default!;
    private const string edgeName = "Edge";
    private const string cornerName = "Corner";
    private const string outerCornerName = "OuterCorner";
    private const int edgeIndex = 0;
    private const int cornerIndex = 1;
    private const int outerCornerIndex = cornerIndex + 1;

    // Bypass nullable everywhere!
    private FrozenDictionary<int, string[]>? tileIdToDecals;
    public static readonly FrozenDictionary<Vector2i, DirectionFlag> dirMapping = new Dictionary<Vector2i, DirectionFlag>()
    {
        [Vector2i.Zero] = DirectionFlag.None,
        [Vector2i.Up] = DirectionFlag.North,
        [Vector2i.Down] = DirectionFlag.South,
        [Vector2i.Left] = DirectionFlag.West,
        [Vector2i.Right] = DirectionFlag.East,
        [Vector2i.DownLeft] = DirectionFlag.SouthWest,
        [Vector2i.DownRight] = DirectionFlag.SouthEast,
        [Vector2i.UpLeft] = DirectionFlag.NorthWest,
        [Vector2i.UpRight] = DirectionFlag.NorthEast,
    }.ToFrozenDictionary();
    public static readonly FrozenDictionary<DirectionFlag, Vector2i> vectorMapping = new Dictionary<DirectionFlag, Vector2i>()
    {
        [DirectionFlag.None] = Vector2i.Zero,
        [DirectionFlag.North] = Vector2i.Up,
        [DirectionFlag.South] = Vector2i.Down,
        [DirectionFlag.West] = Vector2i.Left,
        [DirectionFlag.East] = Vector2i.Right,
        [DirectionFlag.SouthWest] = Vector2i.DownLeft,
        [DirectionFlag.SouthEast] = Vector2i.DownRight,
        [DirectionFlag.NorthWest] = Vector2i.UpLeft,
        [DirectionFlag.NorthEast] = Vector2i.UpRight,
    }.ToFrozenDictionary();
    // Value packing doesn't work :((( SPCR 2025
    private static readonly FrozenDictionary<DirectionFlag, Tuple<int, Angle>> dirToIndexAndRot = new Dictionary<DirectionFlag, Tuple<int, Angle>>()
    {
        [DirectionFlag.North] = new Tuple<int, Angle>(edgeIndex, Angle.FromDegrees(0)),
        [DirectionFlag.East] = new Tuple<int, Angle>(edgeIndex, Angle.FromDegrees(-90)),
        [DirectionFlag.South] = new Tuple<int, Angle>(edgeIndex, Angle.FromDegrees(-180)),
        [DirectionFlag.West] = new Tuple<int, Angle>(edgeIndex, Angle.FromDegrees(-270)),
        [DirectionFlag.NorthEast] = new Tuple<int, Angle>(cornerIndex, Angle.FromDegrees(0)),
        [DirectionFlag.SouthEast] = new Tuple<int, Angle>(cornerIndex, Angle.FromDegrees(-90)),
        [DirectionFlag.SouthWest] = new Tuple<int, Angle>(cornerIndex, Angle.FromDegrees(-180)),
        [DirectionFlag.NorthWest] = new Tuple<int, Angle>(cornerIndex, Angle.FromDegrees(-270)),
    }.ToFrozenDictionary();


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlacementTileEvent>(OnTilePlaced);
        SubscribeLocalEvent<PostInitEvent>(TileInitialize);
    }
    // its coal but its needed :( SPCR 2025
    public void TileInitialize(ref PostInitEvent placementEvent)
    {
        Dictionary<int, string[]> tileMappings = new();
        foreach (var tile in _prototypeManager.EnumeratePrototypes<ContentTileDefinition>())
        {
            if (tile.Directionals is null || tile.Directionals == "")
                continue;
            string[] directionals = new string[3]{tile.Directionals, tile.Directionals, tile.Directionals};
            directionals[edgeIndex] += edgeName;
            directionals[cornerIndex] += cornerName;
            directionals[outerCornerIndex] += outerCornerName;
            tileMappings.Add(tile.TileId, directionals);
        }
        tileIdToDecals = tileMappings.ToFrozenDictionary();

    }
    // First return is cardinals (N,S,E,W) , second is corners (NE, NW, SW, SE)

    private (DirectionFlag, DirectionFlag) getConnectedDirections(MapGridComponent map, Vector2i tileCoordinates, int tileType)
    {
        DirectionFlag directions = DirectionFlag.None;
        DirectionFlag cornerDirections = DirectionFlag.None;
        foreach (var (key, direction) in dirMapping)
        {
            if (!_mapSystem.TryGetTile(map, tileCoordinates + key, out var tile))
                continue;
            if (tile.TypeId != tileType)
                continue;
            // decide if it goes into corners or cardinals
            if (BitOperations.PopCount((byte)direction) == 2)
                cornerDirections |= direction;
            else
                directions |= direction;
        }
        return (directions, cornerDirections);
    }
    private void updateTile(MapGridComponent map, EntityCoordinates tileCoordinates, int tileType)
    {
        // so fucking linter shuts the fuck up
        if (!tileIdToDecals!.ContainsKey(tileType))
            return;
        // stolen straight from DecalPlacementSystem
        var newPos = new Vector2(
            (float) (MathF.Round(tileCoordinates.X - 0.5f, MidpointRounding.AwayFromZero) + 0.5),
            (float) (MathF.Round(tileCoordinates.Y - 0.5f, MidpointRounding.AwayFromZero) + 0.5)
        );
        var coords = tileCoordinates.WithPosition(newPos);
        coords = coords.Offset(new Vector2(-0.5f, -0.5f));
        // get rid of old directionals.
        foreach (var (decalId, decal) in _decalSystem.GetDecalsInRange(
            tileCoordinates.EntityId,
            tileCoordinates.Position))
        {
            // don't delete non-directionals.
            if (!decal.Directional)
                continue;
            _decalSystem.RemoveDecal(tileCoordinates.EntityId, decalId);
        }
        (DirectionFlag ConnectedDirections,DirectionFlag ConnectedCorners) = getConnectedDirections(map, tileCoordinates.ToVector2i(EntityManager,_mapManager, _transformSystem), tileType );
        DirectionFlag DisconnectedDirections = ~ConnectedDirections;
        DirectionFlag DisconnectedCorners = ~ConnectedCorners;
        // do the actual directions now.
        foreach (DirectionFlag dir in Enum.GetValues<DirectionFlag>())
        {
            // Corner dir
            if (BitOperations.PopCount((byte) dir) == 2)
            {
                // For the corner to exist , there should only be the cardinals connected. If the diagonal is connected , it means its filled in
                if ((ConnectedCorners & dir) != dir && (ConnectedDirections & dir) == dir)
                {
                    Decal interiorCorner = new Decal(
                        coords.Position,
                        tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1 + 1], // its the inner corner
                        null,
                        dirToIndexAndRot[dir].Item2,
                        dirToIndexAndRot[dir]
                            .Item1, // z-level can remain same , inner and outer corners shouldn't be in the same corner.
                        false);
                    interiorCorner.Directional = true;
                    if (!_decalSystem.TryAddDecal(interiorCorner, tileCoordinates, out var _))
                        Logger.Error($"Missing decal {tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1]} for tileId {tileType}!");
                }

                if ((DisconnectedDirections & dir) == dir)
                {
                    Decal outerCorner = new Decal(
                        coords.Position,
                        tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1]!,
                        null,
                        dirToIndexAndRot[dir].Item2,
                        dirToIndexAndRot[dir].Item1,
                        false);
                    outerCorner.Directional = true;
                    if(_decalSystem.TryAddDecal(outerCorner, tileCoordinates, out var _))
                        Logger.Error($"Missing decal {tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1]} for tileId {tileType}!");
                }
            }
            else // edge case
            {

                if ((DisconnectedDirections & dir) == DirectionFlag.None)
                    continue;
                Decal neededDecal = new Decal(
                    coords.Position,
                    tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1],
                    null,
                    dirToIndexAndRot[dir].Item2,
                    dirToIndexAndRot[dir].Item1,
                    false);
                neededDecal.Directional = true;
                if (_decalSystem.TryAddDecal(neededDecal, tileCoordinates, out var _))
                    continue;
                Logger.Error(
                    $"Missing decal {tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1]} for tileId {tileType}!");
            }
        }
    }

    private void updateNeighbors(MapGridComponent map, EntityCoordinates tileCoordinates, int tileType)
    {
        foreach (var (key, direction) in dirMapping)
        {
            // yeah no double updating!
            if (key == Vector2i.Zero)
                continue;
            if (!_mapSystem.TryGetTile(
                map,
                tileCoordinates.ToVector2i(EntityManager, _mapManager, _transformSystem) + key,
                out var tile))
                continue;
            if (tile.TypeId != tileType)
                continue;
            updateTile(map, tileCoordinates.Offset(key), tileType);
        }
    }

    private void OnTilePlaced(PlacementTileEvent ev)
    {
        if (!TryComp<MapGridComponent>(ev.Coordinates.EntityId, out var map))
            return;
        updateTile(map, ev.Coordinates, ev.TileType);
        updateNeighbors(map, ev.Coordinates, ev.TileType);
    }


}
