using Robust.Shared.Configuration;
using Robust.Shared.Utility;


namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    public static readonly CVarDef<bool> SalvageMagnetEnabled =
        CVarDef.Create("asteroid_field.salvage_magnet_enabled", false, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<bool> AsteroidFieldEnabled =
        CVarDef.Create("asteroid_field.enabled", true, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<bool> AsteroidFieldSpawnBeacon =
        CVarDef.Create("asteroid_field.beacon_enabled", true, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<ResPath> AsteroidFieldBeaconGridPath =
        CVarDef.Create("asteroid_field.beacon_grid_path", new ResPath("/Maps/_White/Shuttles/asteroid_beacon.yml"), CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<string> AsteroidFieldBeaconName =
        CVarDef.Create("asteroid_field.beacon_name", "asteroid-beacon-name", CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<float> AsteroidFieldDistanceMin =
        CVarDef.Create("asteroid_field.min_distance", 2000f, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<float> AsteroidFieldDistanceMax =
        CVarDef.Create("asteroid_field.max_distance", 2000f, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<int> AsteroidFieldAsteroidCount =
        CVarDef.Create("asteroid_field.asteroid_count", 15, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<int> AsteroidFieldDerelictCount =
        CVarDef.Create("asteroid_field.derelict.count", 5, CVar.SERVER | CVar.ARCHIVE);
}
