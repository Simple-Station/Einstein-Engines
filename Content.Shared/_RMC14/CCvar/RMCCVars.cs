using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.CCVar;

[CVarDefs]
public sealed class RMCCVars : CVars
{
    public static readonly CVarDef<bool> RMCGunPrediction =
        CVarDef.Create("rmc.gun_prediction", true, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<bool> RMCGunPredictionPreventCollision =
        CVarDef.Create("rmc.gun_prediction_prevent_collision", false, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<bool> RMCGunPredictionLogHits =
        CVarDef.Create("rmc.gun_prediction_log_hits", false, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> RMCGunPredictionCoordinateDeviation =
        CVarDef.Create("rmc.gun_prediction_coordinate_deviation", 0.75f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> RMCGunPredictionLowestCoordinateDeviation =
        CVarDef.Create("rmc.gun_prediction_lowest_coordinate_deviation", 0.5f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> RMCGunPredictionAabbEnlargement =
        CVarDef.Create("rmc.gun_prediction_aabb_enlargement", 0.3f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<int> RMCLagCompensationMilliseconds =
        CVarDef.Create("rmc.lag_compensation_milliseconds", 750, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<float> RMCLagCompensationMarginTiles =
        CVarDef.Create("rmc.lag_compensation_margin_tiles", 0.25f, CVar.REPLICATED | CVar.SERVER);
}
