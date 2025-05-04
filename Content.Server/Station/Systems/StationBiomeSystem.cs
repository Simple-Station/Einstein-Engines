using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Shared.Parallax.Biomes;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Station.Systems;
public sealed partial class StationBiomeSystem : EntitySystem
{
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationBiomeComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<StationBiomeComponent> map, ref StationPostInitEvent args)
    {
        if (!TryComp(map, out StationDataComponent? dataComp))
            return;

        var station = _station.GetLargestGrid(dataComp);
        if (station == null) return;

        var mapId = Transform(station.Value).MapID;
        if (!_mapSystem.TryGetMap(mapId, out var mapUid)
            || mapUid is null) // WHAT, IT'S LITERALLY CALLED TRYGET. WHY DO I NEED TO CHECK NULL?
            return;

        _biome.EnsurePlanet(mapUid!.Value, _proto.Index(map.Comp.Biome), map.Comp.Seed, mapLight: map.Comp.MapLightColor);

        if (!TryComp<BiomeComponent>(mapUid, out var biomeComp))
            return; // Yea we JUST made a biome component one line above this trycomp. It turns out I need an engine PR to retrieve the component just added.
                    // Imagine my frustration. On the other hand AddComps like above aren't guaranteed to return a component anyway.

        foreach (var layer in map.Comp.BiomeLayers)
        {
            if (biomeComp.MarkerLayers.Contains(layer))
                continue;

            biomeComp.MarkerLayers.Add(layer);
            biomeComp.ForcedMarkerLayers.Add(layer);
        }
        if (!TryComp(mapUid, out MapGridComponent? mapGrid))
            return;

        foreach (var dungeonProto in map.Comp.Dungeons)
        {
            // TODO: Pester TCJ about adding a _random.NextVector2i to Supermatter Engine, as well as adding methods for "officially" casting vector 2s as integer vectors.
            var distVector = (Vector2i) _random.NextVector2(map.Comp.DungeonMinDistance, map.Comp.DungeonMaxDistance).Rounded();
            _dungeon.GenerateDungeon(dungeonProto, mapUid!.Value, mapGrid, distVector, _random.Next());
        }
    }
}
