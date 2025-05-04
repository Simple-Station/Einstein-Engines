using Robust.Shared.Configuration;

// ReSharper disable once CheckNamespace
namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Should the Lavaland roundstart generation be enabled.
    /// </summary>
    public static readonly CVarDef<bool> LavalandEnabled =
#if RELEASE
        CVarDef.Create("lavaland.enabled", true, CVar.SERVERONLY);
#else //Lavaland murders our test times. If you wanna test lavaland yourself, turn the CVar on manually.
        CVarDef.Create("lavaland.enabled", false, CVar.SERVERONLY);
#endif //Don't worry, this is JUST the worldgen, map tests and grid tests still run just fine.
    public static readonly CVarDef<bool> AllowDuplicatePkaModules =
        CVarDef.Create("modkit.dupes_enabled", true, CVar.REPLICATED | CVar.SERVER);
}
