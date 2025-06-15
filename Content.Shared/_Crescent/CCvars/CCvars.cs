using Robust.Shared.Configuration;

namespace Content.Shared._Crescent.CCvars;

[CVarDefs]
public sealed class CrescentCVars
{
    /// <summary>
    /// Whether or not respawning is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RespawnEnabled =
        CVarDef.Create("sc.respawn.enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Respawn time, how long the player has to wait in seconds after death. 
    /// 
    /// HULLROT NOTE: this does NOT work. use the timer in Content.Shared/CCVar/CCVars.GhostRespawn.cs
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("sc.respawn.time", 69.0f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Enforce role whitelists
    /// </summary>
    public static readonly CVarDef<bool> RoleWhitelist =
        CVarDef.Create("sc.role_whitelist", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// UI Layouts. Here to not conflict with other codebases. (They all share the same config file lol :csgrad:)
    /// </summary>
    public static readonly CVarDef<string> UILayout =
        CVarDef.Create("sc.ui.layout", "Classic", CVar.CLIENTONLY | CVar.ARCHIVE);
}
