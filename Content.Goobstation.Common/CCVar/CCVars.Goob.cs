using Robust.Shared.Configuration;

namespace Content.Goobstation.Common.CCVar;

[CVarDefs]
public sealed partial class GoobCVars
{
    /// <summary>
    /// Controls how often GPS updates.
    /// </summary>
    public static readonly CVarDef<float> GpsUpdateRate =
        CVarDef.Create("gps.update_rate", 1f, CVar.SERVER | CVar.REPLICATED);
}
