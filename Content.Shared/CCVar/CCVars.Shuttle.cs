using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    // Look this is technically eye behavior but its main impact is shuttles so I just dumped it here.
    /// <summary>
    /// If true then the camera will match the grid / map and is unchangeable.
    /// - When traversing grids it will snap to 0 degrees rotation.
    /// False means the player has control over the camera rotation.
    /// - When traversing grids it will snap to the nearest cardinal which will generally be imperceptible.
    /// </summary>
    public static readonly CVarDef<bool> CameraRotationLocked =
        CVarDef.Create("shuttle.camera_rotation_locked", false, CVar.REPLICATED);

    /// <summary>
    /// Whether the arrivals terminal should be on a planet map.
    /// </summary>
    public static readonly CVarDef<bool> ArrivalsPlanet =
        CVarDef.Create("shuttle.arrivals_planet", false, CVar.SERVERONLY);

    /// <summary>
    /// Whether the arrivals shuttle is enabled.
    /// </summary>
    public static readonly CVarDef<bool> ArrivalsShuttles =
        CVarDef.Create("shuttle.arrivals", true, CVar.SERVERONLY);

    /// <summary>
    /// The map to use for the arrivals station.
    /// </summary>
    public static readonly CVarDef<string> ArrivalsMap =
        CVarDef.Create("shuttle.arrivals_map", "/Maps/Misc/terminal.yml", CVar.SERVERONLY);

    /// <summary>
    /// Cooldown between arrivals departures. This should be longer than the FTL time or it will double cycle.
    /// </summary>
    public static readonly CVarDef<float> ArrivalsCooldown =
        CVarDef.Create("shuttle.arrivals_cooldown", 50f, CVar.SERVERONLY);

    /// <summary>
    /// Time it takes the shuttle to spin up it's hyper drive and jump
    /// </summary>
    public static readonly CVarDef<float> ArrivalsStartupTime=
        CVarDef.Create("shuttle.arrivals_startup_time", 5.5f, CVar.SERVERONLY);

    /// <summary>
    /// Time spent in hyperspace
    /// </summary>
    public static readonly CVarDef<float> ArrivalsHyperspaceTime =
        CVarDef.Create("shuttle.arrivals_hyperspace_time", 20f, CVar.SERVERONLY);

    /// <summary>
    /// Are players allowed to return on the arrivals shuttle.
    /// </summary>
    public static readonly CVarDef<bool> ArrivalsReturns =
        CVarDef.Create("shuttle.arrivals_returns", false, CVar.SERVERONLY);

    /// <summary>
    /// Should all players who spawn at arrivals have godmode until they leave the map?
    /// </summary>
    public static readonly CVarDef<bool> GodmodeArrivals =
        CVarDef.Create("shuttle.godmode_arrivals", false, CVar.SERVERONLY);

    /// <summary>
    /// If a grid is split then hide any smaller ones under this mass (kg) from the map.
    /// This is useful to avoid split grids spamming out labels.
    /// </summary>
    public static readonly CVarDef<int> HideSplitGridsUnder =
        CVarDef.Create("shuttle.hide_split_grids_under", 30, CVar.SERVERONLY);

    /// <summary>
    /// Whether to automatically spawn escape shuttles.
    /// </summary>
    public static readonly CVarDef<bool> GridFill =
        CVarDef.Create("shuttle.grid_fill", true, CVar.SERVERONLY);

    /// <summary>
    /// Whether to automatically preloading grids by GridPreloaderSystem
    /// </summary>
    public static readonly CVarDef<bool> PreloadGrids =
        CVarDef.Create("shuttle.preload_grids", true, CVar.SERVERONLY);

    /// <summary>
    /// How long the warmup time before FTL start should be.
    /// </summary>
    public static readonly CVarDef<float> FTLStartupTime =
        CVarDef.Create("shuttle.startup_time", 5.5f, CVar.SERVERONLY);

    /// <summary>
    /// How long a shuttle spends in FTL.
    /// </summary>
    public static readonly CVarDef<float> FTLTravelTime =
        CVarDef.Create("shuttle.travel_time", 20f, CVar.SERVERONLY);

    /// <summary>
    /// How long the final stage of FTL before arrival should be.
    /// </summary>
    public static readonly CVarDef<float> FTLArrivalTime =
        CVarDef.Create("shuttle.arrival_time", 5f, CVar.SERVERONLY);

    /// <summary>
    /// How much time needs to pass before a shuttle can FTL again.
    /// </summary>
    public static readonly CVarDef<float> FTLCooldown =
        CVarDef.Create("shuttle.cooldown", 10f, CVar.SERVERONLY);

    /// <summary>
    /// The maximum <see cref="PhysicsComponent.Mass"/> a grid can have before it becomes unable to FTL.
    /// Any value equal to or less than zero will disable this check.
    /// </summary>
    public static readonly CVarDef<float> FTLMassLimit =
        CVarDef.Create("shuttle.mass_limit", 300f, CVar.SERVERONLY);

    /// <summary>
    /// How long to knock down entities for if they aren't buckled when FTL starts and stops.
    /// </summary>
    public static readonly CVarDef<float> HyperspaceKnockdownTime =
        CVarDef.Create("shuttle.hyperspace_knockdown_time", 5f, CVar.SERVERONLY);

    ///<summary>
    /// the Entropic decay of energy combined with the minimal but existent particles that would slow the vessel down
    ///</summary>
    public static readonly CVarDef<float> SpaceFrictionStrength =
        CVarDef.Create("shuttle.space_friction_strength", 0.0015f, CVar.REPLICATED);

    ///<summary>
    /// the strength of drag when the inertia dampeners are set to anchor to slow the vessel down and hold it in place
    ///</summary>
    public static readonly CVarDef<float> AnchorDampeningStrength =
        CVarDef.Create("shuttle.ancho_dampening_strength", 0.5f, CVar.REPLICATED);

    /// <summary>
    /// Is the emergency shuttle allowed to be early launched.
    /// </summary>
    public static readonly CVarDef<bool> EmergencyEarlyLaunchAllowed =
        CVarDef.Create("shuttle.emergency_early_launch_allowed", false, CVar.SERVERONLY);

    /// <summary>
    /// How long the emergency shuttle remains docked with the station, in seconds.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleDockTime =
        CVarDef.Create("shuttle.emergency_dock_time", 240f, CVar.SERVERONLY);

    /// <summary>
    /// How long after the console is authorized for the shuttle to early launch.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleAuthorizeTime =
        CVarDef.Create("shuttle.emergency_authorize_time", 10f, CVar.SERVERONLY);

    /// <summary>
    /// The minimum time for the emergency shuttle to arrive at centcomm.
    /// Actual minimum travel time cannot be less than <see cref="ShuttleSystem.DefaultArrivalTime"/>
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleMinTransitTime =
        CVarDef.Create("shuttle.emergency_transit_time_min", 90f, CVar.SERVERONLY);

    /// <summary>
    /// The maximum time for the emergency shuttle to arrive at centcomm.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleMaxTransitTime =
        CVarDef.Create("shuttle.emergency_transit_time_max", 180f, CVar.SERVERONLY);

    /// <summary>
    /// Whether the emergency shuttle is enabled or should the round just end.
    /// </summary>
    public static readonly CVarDef<bool> EmergencyShuttleEnabled =
        CVarDef.Create("shuttle.emergency", true, CVar.SERVERONLY);

    /// <summary>
    ///     The percentage of time passed from the initial call to when the shuttle can no longer be recalled.
    ///     ex. a call time of 10min and turning point of 0.5 means the shuttle cannot be recalled after 5 minutes.
    /// </summary>
    public static readonly CVarDef<float> EmergencyRecallTurningPoint =
        CVarDef.Create("shuttle.recall_turning_point", 0.5f, CVar.SERVERONLY);

    /// <summary>
    ///     Time in minutes after round start to auto-call the shuttle. Set to zero to disable.
    /// </summary>
    public static readonly CVarDef<int> EmergencyShuttleAutoCallTime =
        CVarDef.Create("shuttle.auto_call_time", 120, CVar.SERVERONLY);

    /// <summary>
    ///     Time in minutes after the round was extended (by recalling the shuttle) to call
    ///     the shuttle again.
    /// </summary>
    public static readonly CVarDef<int> EmergencyShuttleAutoCallExtensionTime =
        CVarDef.Create("shuttle.auto_call_extension_time", 30, CVar.SERVERONLY);
}
