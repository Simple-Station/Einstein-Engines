using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

[CVarDefs]
public sealed partial class WhiteCVars
{
    public static readonly CVarDef<bool> PremiumActive =
        CVarDef.Create("white.funny_features_enabled", false, CVar.CLIENT | CVar.ARCHIVE);

    public static readonly CVarDef<bool> PMMEnabled =
        CVarDef.Create("pmm.enabled", true, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<bool> PixelSnapCamera =
    	CVarDef.Create("experimental.pixel_snap_camera", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
