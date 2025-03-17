using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Whether pipes will unanchor on ANY conflicting connection. May break maps.
    ///     If false, allows you to stack pipes as long as new directions are added (i.e. in a new pipe rotation, layer or multi-Z link), otherwise unanchoring them.
    /// </summary>
    public static readonly CVarDef<bool> StrictPipeStacking =
        CVarDef.Create("atmos.strict_pipe_stacking", false, CVar.SERVERONLY);

    /// <summary>
    ///     Whether gas differences will move entities.
    /// </summary>
    public static readonly CVarDef<bool> SpaceWind =
        CVarDef.Create("atmos.space_wind", true, CVar.SERVERONLY);

    /// <summary>
    ///     A direct multiplier on how violent space wind is.
    /// </summary>
    public static readonly CVarDef<float> SpaceWindStrengthMultiplier =
        CVarDef.Create("atmos.space_wind_strength_multiplier", 1f, CVar.SERVERONLY);

    /// <summary>
    ///     The maximum velocity (not force) that may be applied to an object by atmospheric pressure differences.
    ///     Useful to prevent clipping through objects.
    /// </summary>
    public static readonly CVarDef<float> SpaceWindMaxVelocity =
        CVarDef.Create("atmos.space_wind_max_velocity", 15f, CVar.SERVERONLY);

    /// <summary>
    ///     The maximum angular velocity that space wind can spin objects at while throwing them. This one is mostly for fun.
    /// </summary>
    public static readonly CVarDef<float> SpaceWindMaxAngularVelocity =
        CVarDef.Create("atmos.space_wind_max_angular_velocity", 3f, CVar.SERVERONLY);

    /// <summary>
    ///     The amount of time (in seconds) for space wind to knock down a player character if they are subjected to space wind.
    /// </summary>
    public static readonly CVarDef<float> SpaceWindKnockdownTime =
        CVarDef.Create("atmos.space_wind_knockdown_time", 1f, CVar.SERVERONLY);

    /// <summary>
    ///     Whether monstermos tile equalization is enabled.
    /// </summary>
    public static readonly CVarDef<bool> MonstermosEqualization =
        CVarDef.Create("atmos.monstermos_equalization", true, CVar.SERVERONLY);

    /// <summary>
    ///     Whether monstermos explosive depressurization is enabled.
    ///     Needs <see cref="MonstermosEqualization"/> to be enabled to work.
    /// </summary>
    public static readonly CVarDef<bool> MonstermosDepressurization =
        CVarDef.Create("atmos.monstermos_depressurization", true, CVar.SERVERONLY);

    /// <summary>
    ///     Whether monstermos explosive depressurization will rip tiles..
    ///     Needs <see cref="MonstermosEqualization"/> and <see cref="MonstermosDepressurization"/> to be enabled to work.
    ///     WARNING: This cvar causes MAJOR contrast issues, and usually tends to make any spaced scene look very cluttered.
    ///     This not only usually looks strange, but can also reduce playability for people with impaired vision. Please think twice before enabling this on your server.
    ///     Also looks weird on slow spacing for unrelated reasons. If you do want to enable this, you should probably turn on instaspacing.
    /// </summary>
    public static readonly CVarDef<bool> MonstermosRipTiles =
        CVarDef.Create("atmos.monstermos_rip_tiles", false, CVar.SERVERONLY);

    /// <summary>
    ///     Taken as the cube of a tile's mass, this acts as a minimum threshold of mass for which air pressure calculates whether or not to rip a tile from the floor
    ///     This should be set by default to the cube of the game's lowest mass tile as defined in their prototypes, but can be increased for server performance reasons
    /// </summary>
    public static readonly CVarDef<float> MonstermosRipTilesMinimumPressure =
        CVarDef.Create("atmos.monstermos_rip_tiles_min_pressure", 20f, CVar.SERVERONLY);

    /// <summary>
    ///     Whether explosive depressurization will cause the grid to gain an impulse.
    ///     Needs <see cref="MonstermosEqualization"/> and <see cref="MonstermosDepressurization"/> to be enabled to work.
    /// </summary>
    public static readonly CVarDef<bool> AtmosGridImpulse =
        CVarDef.Create("atmos.grid_impulse", false, CVar.SERVERONLY);

    /// <summary>
    ///     Minimum amount of air allowed on a spaced tile before it is reset to 0 immediately in kPa
    ///     Since the decay due to SpacingEscapeRatio follows a curve, it would never reach 0.0 exactly
    ///     unless we truncate it somewhere.
    /// </summary>
    public static readonly CVarDef<float> AtmosSpacingMinGas =
        CVarDef.Create("atmos.mmos_min_gas", 2.0f, CVar.SERVERONLY);

    /// <summary>
    ///     How much wind can go through a single tile before that tile doesn't depressurize itself
    ///     (I.e spacing is limited in large rooms heading into smaller spaces)
    /// </summary>
    public static readonly CVarDef<float> AtmosSpacingMaxWind =
        CVarDef.Create("atmos.mmos_max_wind", 500f, CVar.SERVERONLY);

    /// <summary>
    ///     Whether atmos superconduction is enabled.
    /// </summary>
    /// <remarks> Disabled by default, superconduction is awful. </remarks>
    public static readonly CVarDef<bool> Superconduction =
        CVarDef.Create("atmos.superconduction", false, CVar.SERVERONLY);

    /// <summary>
    ///     Heat loss per tile due to radiation at 20 degC, in W.
    /// </summary>
    public static readonly CVarDef<float> SuperconductionTileLoss =
        CVarDef.Create("atmos.superconduction_tile_loss", 30f, CVar.SERVERONLY);

    /// <summary>
    ///     Whether excited groups will be processed and created.
    /// </summary>
    public static readonly CVarDef<bool> ExcitedGroups =
        CVarDef.Create("atmos.excited_groups", true, CVar.SERVERONLY);

    /// <summary>
    ///     Whether all tiles in an excited group will clear themselves once being exposed to space.
    ///     Similar to <see cref="MonstermosDepressurization"/>, without none of the tile ripping or
    ///     things being thrown around very violently.
    ///     Needs <see cref="ExcitedGroups"/> to be enabled to work.
    /// </summary>
    public static readonly CVarDef<bool> ExcitedGroupsSpaceIsAllConsuming =
        CVarDef.Create("atmos.excited_groups_space_is_all_consuming", false, CVar.SERVERONLY);

    /// <summary>
    ///     Maximum time in milliseconds that atmos can take processing.
    /// </summary>
    public static readonly CVarDef<float> AtmosMaxProcessTime =
        CVarDef.Create("atmos.max_process_time", 3f, CVar.SERVERONLY);

    /// <summary>
    ///     Atmos tickrate in TPS. Atmos processing will happen every 1/TPS seconds.
    /// </summary>
    public static readonly CVarDef<float> AtmosTickRate =
        CVarDef.Create("atmos.tickrate", 15f, CVar.SERVERONLY);

    /// <summary>
    ///     Scale factor for how fast things happen in our atmosphere
    ///     simulation compared to real life. 1x means pumps run at 1x
    ///     speed. Players typically expect things to happen faster
    ///     in-game.
    /// </summary>
    public static readonly CVarDef<float> AtmosSpeedup =
        CVarDef.Create("atmos.speedup", 8f, CVar.SERVERONLY);

    /// <summary>
    ///     Like atmos.speedup, but only for gas and reaction heat values. 64x means
    ///     gases heat up and cool down 64x faster than real life.
    /// </summary>
    public static readonly CVarDef<float> AtmosHeatScale =
        CVarDef.Create("atmos.heat_scale", 8f, CVar.SERVERONLY);

    /// <summary>
    ///     A multiplier on the amount of force applied to Humanoid entities, as tracked by HumanoidAppearanceComponent
    ///     This multiplier is added after all other checks are made, and applies to both throwing force, and how easy it is for an entity to be thrown.
    /// </summary>
    public static readonly CVarDef<float> AtmosHumanoidThrowMultiplier =
        CVarDef.Create("atmos.humanoid_throw_multiplier", 2f, CVar.SERVERONLY);
}
