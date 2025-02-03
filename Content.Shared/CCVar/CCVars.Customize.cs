using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Allow players to add extra pronouns to their examine window.
    ///     It looks something like "She also goes by they/them pronouns."
    /// </summary>
    public static readonly CVarDef<bool> AllowCosmeticPronouns =
        CVarDef.Create("customize.allow_cosmetic_pronouns", false, CVar.REPLICATED);

    /// <summary>
    ///     Allow players to set their own Station AI names.
    /// </summary>
    public static readonly CVarDef<bool> AllowCustomStationAiName =
        CVarDef.Create("customize.allow_custom_station_ai_name", false, CVar.REPLICATED);

    /// <summary>
    ///     Allow players to set their own cyborg names. (borgs, mediborgs, etc)
    /// </summary>
    public static readonly CVarDef<bool> AllowCustomCyborgName =
        CVarDef.Create("customize.allow_custom_cyborg_name", false, CVar.REPLICATED);
}
