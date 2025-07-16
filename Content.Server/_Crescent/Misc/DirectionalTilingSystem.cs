using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Server.Decals;
using Content.Shared.Decals;
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
    [Dependency] private readonly PrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly DecalSystem _decalSystem = default!;
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
        [DirectionFlag.North] = new Tuple<int, Angle>(edgeIndex, 0),
        [DirectionFlag.East] = new Tuple<int, Angle>(edgeIndex, 90),
        [DirectionFlag.South] = new Tuple<int, Angle>(edgeIndex, 180),
        [DirectionFlag.West] = new Tuple<int, Angle>(edgeIndex, 270),
        [DirectionFlag.NorthEast] = new Tuple<int, Angle>(cornerIndex, 0),
        [DirectionFlag.SouthEast] = new Tuple<int, Angle>(cornerIndex, 90),
        [DirectionFlag.SouthWest] = new Tuple<int, Angle>(cornerIndex, 180),
        [DirectionFlag.NorthWest] = new Tuple<int, Angle>(cornerIndex, 270),
    }.ToFrozenDictionary();


public override void Initialize()
    {
        base.Initialize();
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

        SubscribeLocalEvent<PlacementTileEvent>(OnTilePlaced);
    }

    private DirectionFlag getConnectedDirections(MapGridComponent map, Vector2i tileCoordinates, int tileType)
    {
        DirectionFlag directions = DirectionFlag.None;
        foreach (var (key, direction) in dirMapping)
        {
            if(_mapSystem.TryGetTile(map, tileCoordinates+key, out var tile) && tile.TypeId == tileType)
                directions |= direction;
        }
        return directions;
    }

    private void OnTilePlaced(PlacementTileEvent ev)
    {
        if (!tileIdToDecals!.ContainsKey(ev.TileType))
            return;
        if (!TryComp<MapGridComponent>(ev.Coordinates.EntityId, out var map))
            return;
        DirectionFlag DisconnectedDirections = ~getConnectedDirections(map, ev.Coordinates.ToVector2i(EntityManager,_mapManager, _transformSystem), ev.TileType );
        foreach (DirectionFlag dir in Enum.GetValues<DirectionFlag>())
        {
            if ((DisconnectedDirections & dir) == DirectionFlag.None)
                continue;
            Decal neededDecal = new Decal(
                Vector2.Zero,
                tileIdToDecals[ev.TileType][dirToIndexAndRot[dir].Item1],
                null,
                dirToIndexAndRot[dir].Item2,
                dirToIndexAndRot[dir].Item1,
                false);
            if(_decalSystem.TryAddDecal(neededDecal, ev.Coordinates, out var _))
                continue;
            Logger.Error($"Missing decal {tileIdToDecals[ev.TileType][dirToIndexAndRot[dir].Item1]} for tileId {ev.TileType}!");
        }
    }


}
