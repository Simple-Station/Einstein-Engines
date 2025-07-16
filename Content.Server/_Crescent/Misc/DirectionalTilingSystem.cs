using System.Collections.Frozen;
using Content.Server.Decals;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Placement;
using Robust.Shared.Prototypes;


namespace Content.Server._Crescent;


/// <summary>
/// SPCR/MLGTASTICa 2025
/// Handles creating the decals to create the illusion of directional tiles.
/// Based off the System used by the Eris SS13 Codebase.
/// This file is licensed under MIT. Feel free to do whatever.
/// </summary>
public sealed class DirectionalTilingSystem : EntitySystem
{
    [Dependency] private readonly TileSystem _tileSystem = default!;
    [Dependency] private readonly PrototypeManager _prototypeManager = default!;
    [Dependency] private readonly DecalSystem _decalSystem = default!;
    private const string edgeName = "Edge";
    private const string cornerName = "Corner";
    private const string outerCornerName = "OuterCorner";
    private const int edgeIndex = 0;
    private const int cornerIndex = 1;
    private const int outerCornerIndex = 2;
    // Bypass nullable everywhere!
    private FrozenDictionary<string, string[]>? tileIdToDecals;
    public override void Initialize()
    {
        base.Initialize();
        Dictionary<string, string[]> tileMappings = new();
        foreach (var tile in _prototypeManager.EnumeratePrototypes<ContentTileDefinition>())
        {
            if (tile.Directionals is null || tile.Directionals == "")
                continue;
            string[] directionals = new string[3]{tile.Directionals, tile.Directionals, tile.Directionals};
            directionals[edgeIndex] += edgeName;
            directionals[cornerIndex] += cornerName;
            directionals[outerCornerIndex] += outerCornerName;
            tileMappings.Add(tile.ID, directionals);
        }
        tileIdToDecals = tileMappings.ToFrozenDictionary();

        SubscribeLocalEvent<PlacementTileEvent>(OnTilePlaced);
    }

    private void OnTilePlaced(PlacementTileEvent ev)
    {
        if(ev.TileType)
    }


}
