using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> AutoGetUp =
        CVarDef.Create("rest.auto_get_up", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    public static readonly CVarDef<bool> HoldLookUp =
        CVarDef.Create("rest.hold_look_up", false, CVar.CLIENT | CVar.ARCHIVE);

    /// <summary>
    ///     When true, players can choose to crawl under tables while laying down, using the designated keybind.
    /// </summary>
    public static readonly CVarDef<bool> CrawlUnderTables =
        CVarDef.Create("rest.crawlundertables", true, CVar.SERVER | CVar.ARCHIVE);
}
