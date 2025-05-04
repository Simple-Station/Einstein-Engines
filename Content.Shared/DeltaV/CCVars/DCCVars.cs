using Robust.Shared.Configuration;

namespace Content.Shared.DeltaV.CCVars;

/// <summary>
/// DeltaV specific cvars.
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming - Shush you
public sealed class DCCVars
{
    /// <summary>
    /// Anti-EORG measure. Will add pacified to all players upon round end.
    /// Its not perfect, but gets the job done.
    /// </summary>
    public static readonly CVarDef<bool> RoundEndPacifist =
        CVarDef.Create("game.round_end_pacifist", false, CVar.SERVERONLY);

    /*
     * No EORG
     */

    /// <summary>
    /// Whether the no EORG popup is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RoundEndNoEorgPopup =
        CVarDef.Create("game.round_end_eorg_popup_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Skip the no EORG popup.
    /// </summary>
    public static readonly CVarDef<bool> SkipRoundEndNoEorgPopup =
        CVarDef.Create("game.skip_round_end_eorg_popup", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long to display the EORG popup for.
    /// </summary>
    public static readonly CVarDef<float> RoundEndNoEorgPopupTime =
        CVarDef.Create("game.round_end_eorg_popup_time", 5f, CVar.SERVER | CVar.REPLICATED);

    /*
     * Auto ACO
     */

    /// <summary>
    /// How long with no captain before requesting an ACO be elected.
    /// </summary>
    public static readonly CVarDef<TimeSpan> RequestAcoDelay =
        CVarDef.Create("game.request_aco_delay", TimeSpan.FromMinutes(15), CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Determines whether an ACO should be requested when the captain leaves during the round,
    /// in addition to cases where there are no captains at round start.
    /// </summary>
    public static readonly CVarDef<bool> RequestAcoOnCaptainDeparture =
        CVarDef.Create("game.request_aco_on_captain_departure", true, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// Determines whether All Access (AA) should be automatically unlocked if no captain is present.
    /// </summary>
    public static readonly CVarDef<bool> AutoUnlockAllAccessEnabled =
        CVarDef.Create("game.auto_unlock_aa_enabled", true, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// How long after an ACO request announcement is made before All Access (AA) should be unlocked.
    /// </summary>
    public static readonly CVarDef<TimeSpan> AutoUnlockAllAccessDelay =
        CVarDef.Create("game.auto_unlock_aa_delay", TimeSpan.FromMinutes(5), CVar.SERVERONLY | CVar.ARCHIVE);

    /*
     * Misc.
     */

    /// <summary>
    /// Whether the Shipyard is enabled.
    /// </summary>
    public static readonly CVarDef<bool> Shipyard =
        CVarDef.Create("shuttle.shipyard", true, CVar.SERVERONLY);
}
