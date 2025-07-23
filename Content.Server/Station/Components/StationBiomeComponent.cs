using Content.Server.Station.Systems;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Markers;
using Content.Shared.Procedural;
using Robust.Shared.Prototypes;

namespace Content.Server.Station.Components;

/// <summary>
/// Runs EnsurePlanet against the largest grid on Mapinit.
/// </summary>
[RegisterComponent, Access(typeof(StationBiomeSystem))]
public sealed partial class StationBiomeComponent : Component
{
    [DataField(required: true)]
    public ProtoId<BiomeTemplatePrototype> Biome = "Grasslands";

    /// <summary>
    ///     Adds a list of biome marker layers after creating the planet. Useful if you wish to make your planet station also have ores to mine.
    /// </summary>
    [DataField]
    public List<ProtoId<BiomeMarkerLayerPrototype>> BiomeLayers;

    /// <summary>
    ///     Whether your station comes with one or more complimentary dungeons somewhere in the world.
    /// </summary>
    [DataField]
    public List<DungeonConfigPrototype> Dungeons;

    [DataField]
    public float DungeonMinDistance = 100f;

    [DataField]
    public float DungeonMaxDistance = 500f;

    // If null, its random
    [DataField]
    public int? Seed = null;

    [DataField]
    public Color MapLightColor = Color.Black;
}
