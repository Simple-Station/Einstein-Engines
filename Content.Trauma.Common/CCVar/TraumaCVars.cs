using Robust.Shared.Configuration;

namespace Content.Trauma.Common.CCVar;

[CVarDefs]
public sealed partial class TraumaCVars
{
    #region Slop

    /// <summary>
    ///     Is antag pity enabled
    /// </summary>
    public static readonly CVarDef<bool> AntagPityEnabled =
        CVarDef.Create("trauma.pity_enabled", false, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Whether to enable the ghost bar.
    /// This is not implemented in the UI, it is just to make tests not take 500 years to run.
    /// </summary>
    public static readonly CVarDef<bool> GhostBarEnabled =
        CVarDef.Create("trauma.ghost_bar_enabled", true, CVar.SERVER | CVar.REPLICATED);

    #endregion

    #region Mining rewards

    /// <summary>
    /// Maximum currency to possibly give a player from mining in a round.
    /// </summary>
    public static readonly CVarDef<int> MiningRewardLimit =
        CVarDef.Create("trauma.mining_reward_limit", 100, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// How many mining points give 1 currency.
    /// </summary>
    public static readonly CVarDef<int> MiningRewardRatio =
        CVarDef.Create("trauma.mining_reward_ratio", 50, CVar.SERVER | CVar.REPLICATED);

    #endregion

    #region AudioMuffle

    /// <summary>
    /// Is audio muffle raycast behavior enabled?
    /// </summary>
    public static readonly CVarDef<bool> AudioMuffleRaycast =
        CVarDef.Create("trauma.audio_muffle_raycast", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Is audio muffle pathfinding behavior enabled?
    /// </summary>
    public static readonly CVarDef<bool> AudioMufflePathfinding =
        CVarDef.Create("trauma.audio_muffle_pathfinding", true, CVar.SERVER | CVar.REPLICATED);

    #endregion

    #region Streamer Mode

    /// <summary>
    /// Client setting to disable music that would cause copyright claims.
    /// </summary>
    public static readonly CVarDef<bool> StreamerMode =
        CVarDef.Create("trauma.streamer_mode", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    #endregion

    #region Gun prediction

    /// <summary>
    /// Distance used between projectile and lag-compensated target position for gun prediction.
    /// </summary>
    public static readonly CVarDef<float> GunLagCompRange =
        CVarDef.Create("trauma.gun_lag_comp_range", 0.6f, CVar.SERVER);

    #endregion
}
