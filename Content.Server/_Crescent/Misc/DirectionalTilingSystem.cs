using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading.Tasks;
using Content.Server.Decals;
using Content.Shared._Crescent;
using Content.Shared.Decals;
using Content.Shared.Entry;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Placement;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;


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
    public static readonly FrozenDictionary<DirectionFlag, string> dirToString = new Dictionary<DirectionFlag, string>()
    {
        [DirectionFlag.North] = "N",
        [DirectionFlag.South] = "S",
        [DirectionFlag.West] = "W",
        [DirectionFlag.East] = "E",
        [DirectionFlag.SouthWest] = "SW",
        [DirectionFlag.SouthEast] = "SE",
        [DirectionFlag.NorthWest] = "NW",
        [DirectionFlag.NorthEast] = "NE",
    }.ToFrozenDictionary();
    // Value packing doesn't work :((( SPCR 2025
    // Used for SIMPLE edges that dont have unique directionals
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
        SubscribeLocalEvent<TileChangedEvent>(OnTileChanged);
        SubscribeLocalEvent<PostInitEvent>(TileInitialize);
        // Remove this when all maps have been updated to save the decals by default , SPCR 2025
        SubscribeLocalEvent<GridInitializeEvent>(GridInitialize);
    }
    // its coal but its needed :( SPCR 2025

    public void GridInitialize(GridInitializeEvent args)
    {
        var mapComp = Comp<MapGridComponent>(args.EntityUid);
        var enumerator = _mapSystem.GetAllTilesEnumerator(args.EntityUid, mapComp, true);
        while (enumerator.MoveNext(out var tile))
        {
            if (!(_tiles[tile.Value.Tile.TypeId] is ContentTileDefinition contentDef))
                continue;
            EntityCoordinates gridPos = _mapSystem.GridTileToLocal(args.EntityUid, mapComp, tile.Value.GridIndices);
            updateTile(
                mapComp,
                gridPos,
                tile.Value.Tile.TypeId,
                contentDef.DirectionalType,
                contentDef.uniqueDirectionals);
        }
    }
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

    public void OnTileChanged(ref TileChangedEvent tileEvent)
    {
        if (!TryComp<MapGridComponent>(tileEvent.Entity, out var map))
            return;
        EntityCoordinates gridPos = _mapSystem.GridTileToLocal(tileEvent.NewTile.GridUid, map, tileEvent.NewTile.GridIndices);
        if (_tiles[tileEvent.OldTile.TypeId] is not ContentTileDefinition contentDefOld)
            return;
        DeleteDirectionalDecals(gridPos);
        if (tileIdToDecals!.ContainsKey(tileEvent.OldTile.TypeId))
            updateNeighbors(map, gridPos, tileEvent.OldTile.TypeId, contentDefOld.DirectionalType, contentDefOld.uniqueDirectionals, true);
        if (!tileIdToDecals!.ContainsKey(tileEvent.NewTile.Tile.TypeId))
            return;
        if (_tiles[tileEvent.NewTile.Tile.TypeId] is not ContentTileDefinition contentDef)
            return;
        updateTile(map, gridPos, tileEvent.NewTile.Tile.TypeId, contentDef.DirectionalType, contentDef.uniqueDirectionals, true);
        updateNeighbors(map, gridPos, tileEvent.NewTile.Tile.TypeId, contentDef.DirectionalType, contentDef.uniqueDirectionals, false);


    }

    public void DeleteDirectionalDecals(EntityCoordinates tileCoordinates)
    {
        foreach (var (decalId, decal) in _decalSystem.GetDecalsInRange(
            tileCoordinates.EntityId,
            tileCoordinates.Position))
        {
            // don't delete non-directionals.
            if (!decal.Directional)
                continue;
            _decalSystem.RemoveDecal(tileCoordinates.EntityId, decalId);
        }
    }
    // First return is cardinals (N,S,E,W) , second is corners (NE, NW, SW, SE)

    private (DirectionFlag, DirectionFlag) getConnectedDirections(MapGridComponent map, Vector2i tileCoordinates, int tileType, DirectionalType directionalType)
    {
        DirectionFlag directions = DirectionFlag.None;
        DirectionFlag cornerDirections = DirectionFlag.None;
        foreach (var (key, direction) in dirMapping)
        {
            if (!_mapSystem.TryGetTile(map, tileCoordinates + key, out var tile))
            {
                if(directionalType == DirectionalType.ExistReversed)
                    if (BitOperations.PopCount((byte)direction) == 2)
                        cornerDirections |= direction;
                    else
                        directions |= direction;
                continue;
            }

            if (directionalType == DirectionalType.Same && tile.TypeId != tileType )
                continue;
            if (directionalType == DirectionalType.Exist && tile.TypeId == Tile.Empty.TypeId)
                continue;
            if (directionalType == DirectionalType.ExistReversed && tile.TypeId != Tile.Empty.TypeId)
                continue;
            // decide if it goes into corners or cardinals
            if (BitOperations.PopCount((byte)direction) == 2)
                cornerDirections |= direction;
            else
                directions |= direction;
        }
        return (directions, cornerDirections);
    }
    private void updateTile(MapGridComponent map, EntityCoordinates tileCoordinates, int tileType, DirectionalType directionalType, bool uniqueDirectionals = false, bool special = false)
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
        DeleteDirectionalDecals(tileCoordinates);
        (DirectionFlag ConnectedDirections,DirectionFlag ConnectedCorners) = getConnectedDirections(map, tileCoordinates.ToVector2i(EntityManager,_mapManager, _transformSystem), tileType, directionalType );
        DirectionFlag DisconnectedDirections = ~ConnectedDirections;
        // do the actual directions now.
        foreach (DirectionFlag dir in Enum.GetValues<DirectionFlag>())
        {
            if (dir == DirectionFlag.None)
                continue;
            // Corner dir
            if (BitOperations.PopCount((byte) dir) == 2)
            {
                // For the corner to exist , there should only be the cardinals connected. If the diagonal is connected , it means its filled in
                if ((ConnectedCorners & dir) != dir && (ConnectedDirections & dir) == dir)
                {
                    // Reversed so this also has to be reversed a bit to prevent a edge case.
                    if (directionalType == DirectionalType.ExistReversed)
                        continue;
                    Decal interiorCorner = new Decal(
                        coords.Position,
                        tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1 + 1] + (uniqueDirectionals ? dirToString[dir] : ""), // its the inner corner
                        null,
                        !uniqueDirectionals ? dirToIndexAndRot[dir].Item2 : Angle.Zero,
                        dirToIndexAndRot[dir].Item1, // z-level can remain same , inner and outer corners shouldn't be in the same corner.
                        false);
                    interiorCorner.Directional = true;
                    if (!_decalSystem.TryAddDecal(interiorCorner, tileCoordinates, out var _))
                        Logger.Error($"Missing decal {tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1+1] + (uniqueDirectionals ? dirToString[dir] : "" )} for tileId {tileType}!");
                }
                // disconnected dont care about fill in.
                if ((DisconnectedDirections & dir) == dir)
                {
                    Decal outerCorner = new Decal(
                        coords.Position,
                        tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1] + (uniqueDirectionals ? dirToString[dir] : ""),
                        null,
                        !uniqueDirectionals ? dirToIndexAndRot[dir].Item2 : Angle.Zero,
                        dirToIndexAndRot[dir].Item1,
                        false);
                    outerCorner.Directional = true;
                    if(!_decalSystem.TryAddDecal(outerCorner, tileCoordinates, out var _))
                        Logger.Error($"Missing decal {tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1] + (uniqueDirectionals ? dirToString[dir] : "")} for tileId {tileType}!");
                }
            }
            else // edge case
            {

                if ((DisconnectedDirections & dir) == DirectionFlag.None)
                    continue;
                Decal neededDecal = new Decal(
                    coords.Position,
                    tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1] + (uniqueDirectionals ? dirToString[dir] : ""),
                    null,
                    !uniqueDirectionals ? dirToIndexAndRot[dir].Item2 : Angle.Zero,
                    dirToIndexAndRot[dir].Item1,
                    false);
                neededDecal.Directional = true;
                if (_decalSystem.TryAddDecal(neededDecal, tileCoordinates, out var _))
                    continue;
                Logger.Error(
                    $"Missing decal {tileIdToDecals[tileType][dirToIndexAndRot[dir].Item1] + (uniqueDirectionals ? dirToString[dir] : "")} for tileId {tileType}!");
            }
        }
    }

    private void updateNeighbors(MapGridComponent map, EntityCoordinates tileCoordinates, int tileType, DirectionalType directionalType, bool uniqueDirectionals = false, bool requireSame = true)
    {
        foreach (var (key, direction) in dirMapping)
        {
            if (key == Vector2i.Zero)
                continue;
            if (!_mapSystem.TryGetTile(map, tileCoordinates.ToVector2i(EntityManager, _mapManager, _transformSystem) + key, out var tile))
                continue;
            if (tile.TypeId == tileType)
                updateTile(map, tileCoordinates.Offset(key), tileType, directionalType, uniqueDirectionals);
            else if(!requireSame)
            {
                // not required to be same to count as neighbor. Get their tile def's contentDef and update properly!
                if (tileIdToDecals!.ContainsKey(tile.TypeId))
                    continue;
                if (_tiles[tile.TypeId] is not ContentTileDefinition contentDef)
                    continue;
                updateTile(map, tileCoordinates.Offset(key), tileType, contentDef.DirectionalType, contentDef.uniqueDirectionals);

            }
        }
    }

    private void OnTilePlaced(PlacementTileEvent ev)
    {
        if (!TryComp<MapGridComponent>(ev.Coordinates.EntityId, out var map))
            return;
        if (!tileIdToDecals!.ContainsKey(ev.TileType))
        {
            DeleteDirectionalDecals(ev.Coordinates);
            updateNeighbors(map, ev.Coordinates, ev.TileType, DirectionalType.None, false, false);
            return;
        }

        if (!(_tiles[ev.TileType] is ContentTileDefinition contentDef))
            return;
        updateTile(map, ev.Coordinates, ev.TileType, contentDef.DirectionalType, contentDef.uniqueDirectionals, true);
        updateNeighbors(map, ev.Coordinates, ev.TileType, contentDef.DirectionalType, contentDef.uniqueDirectionals, false);
    }


}
