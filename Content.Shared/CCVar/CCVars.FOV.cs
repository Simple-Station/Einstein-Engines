using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     The number by which the current FOV size is divided for each level.
    /// </summary>
    public static readonly CVarDef<float> ZoomLevelStep =
        CVarDef.Create("fov.zoom_step", 1.2f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     How many times the player can zoom in until they reach the minimum zoom.
    ///     This does not affect the maximum zoom.
    /// </summary>
    public static readonly CVarDef<int> ZoomLevels =
        CVarDef.Create("fov.zoom_levels", 7, CVar.SERVER | CVar.REPLICATED);
}
