using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    /// <summary>
    /// What intensity will the grain shader be at
    /// </summary>
    public static readonly CVarDef<float> FilmGrainStrength =
        CVarDef.Create("graphics.film_grain_strength", 50f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Grain shader on/off
    /// </summary>
    public static readonly CVarDef<bool> FilmGrain =
        CVarDef.Create("graphics.film_grain", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
