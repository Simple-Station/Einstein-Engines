using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    public static readonly CVarDef<bool> EnableLightsGlowing =
        CVarDef.Create("light.enable_lights_glowing", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
