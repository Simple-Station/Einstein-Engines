using Content.Shared.Roles;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Disables most functionality in the GameTicker.
    /// </summary>
    public static readonly CVarDef<bool>
        GameDummyTicker = CVarDef.Create("game.dummyticker", false, CVar.ARCHIVE | CVar.SERVERONLY);

    /// <summary>
    ///     Controls if the lobby is enabled. If it is not, and there are no available jobs, you may get stuck on a black screen.
    /// </summary>
    public static readonly CVarDef<bool>
        GameLobbyEnabled = CVarDef.Create("game.lobbyenabled", true, CVar.ARCHIVE);

    /// <summary>
    ///     Controls the duration of the lobby timer in seconds. Defaults to 2 minutes and 30 seconds.
    /// </summary>
    public static readonly CVarDef<int>
        GameLobbyDuration = CVarDef.Create("game.lobbyduration", 150, CVar.ARCHIVE);

    /// <summary>
    ///     Controls if players can latejoin at all.
    /// </summary>
    public static readonly CVarDef<bool>
        GameDisallowLateJoins = CVarDef.Create("game.disallowlatejoins", false, CVar.ARCHIVE | CVar.SERVERONLY);

    /// <summary>
    ///     Controls the default game preset.
    /// </summary>
    public static readonly CVarDef<string>
        GameLobbyDefaultPreset = CVarDef.Create("game.defaultpreset", "secret", CVar.ARCHIVE);

    /// <summary>
    ///     Controls if the game can force a different preset if the current preset's criteria are not met.
    /// </summary>
    public static readonly CVarDef<bool>
        GameLobbyFallbackEnabled = CVarDef.Create("game.fallbackenabled", true, CVar.ARCHIVE);

    /// <summary>
    ///     The preset for the game to fall back to if the selected preset could not be used, and fallback is enabled.
    /// </summary>
    public static readonly CVarDef<string>
        GameLobbyFallbackPreset = CVarDef.Create("game.fallbackpreset", "Traitor,Extended", CVar.ARCHIVE);

    /// <summary>
    ///     Controls if people can win the game in Suspicion or Deathmatch.
    /// </summary>
    public static readonly CVarDef<bool>
        GameLobbyEnableWin = CVarDef.Create("game.enablewin", true, CVar.ARCHIVE);

    /// <summary>
    ///     Minimum time between Basic station events in seconds
    /// </summary>
    public static readonly CVarDef<int> // 5 Minutes
        GameEventsBasicMinimumTime = CVarDef.Create("game.events_basic_minimum_time", 300, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Maximum time between Basic station events in seconds
    /// </summary>
    public static readonly CVarDef<int> // 25 Minutes
        GameEventsBasicMaximumTime = CVarDef.Create("game.events_basic_maximum_time", 1500, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Minimum time between Ramping station events in seconds
    /// </summary>
    public static readonly CVarDef<int> // 4 Minutes
        GameEventsRampingMinimumTime = CVarDef.Create("game.events_ramping_minimum_time", 240, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Maximum time between Ramping station events in seconds
    /// </summary>
    public static readonly CVarDef<int> // 12 Minutes
        GameEventsRampingMaximumTime = CVarDef.Create("game.events_ramping_maximum_time", 720, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Minimum time between Oscillating station events in seconds. This is the bare minimum which will never be violated, unlike with ramping events.
    /// </summary>
    public static readonly CVarDef<int> // 40 seconds
        GameEventsOscillatingMinimumTime = CVarDef.Create("game.events_oscillating_minimum_time", 40, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Time between Oscillating station events in seconds at 1x chaos level. Events may occur at larger intervals if current chaos is lower than that.
    /// </summary>
    public static readonly CVarDef<int> // 20 Minutes - which constitutes a minimum of 120 seconds between events in Irregular and 280 seconds in Extended Irregular
        GameEventsOscillatingAverageTime = CVarDef.Create("game.events_oscillating_average_time", 1200, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Controls the maximum number of character slots a player is allowed to have.
    /// </summary>
    public static readonly CVarDef<int>
        GameMaxCharacterSlots = CVarDef.Create("game.maxcharacterslots", 30, CVar.ARCHIVE | CVar.SERVERONLY);

    /// <summary>
    ///     Controls the game map prototype to load. SS14 stores these prototypes in Prototypes/Maps.
    /// </summary>
    public static readonly CVarDef<string>
        GameMap = CVarDef.Create("game.map", string.Empty, CVar.SERVERONLY);

    /// <summary>
    /// If roles should be restricted based on whether or not they are whitelisted.
    /// </summary>
    public static readonly CVarDef<bool>
        GameRoleWhitelist = CVarDef.Create("game.role_whitelist", true, CVar.SERVER | CVar.REPLICATED);


    /// <summary>
    ///     Controls whether to use world persistence or not.
    /// </summary>
    public static readonly CVarDef<bool>
        UsePersistence = CVarDef.Create("game.usepersistence", false, CVar.ARCHIVE);

    /// <summary>
    ///     If world persistence is used, what map prototype should be initially loaded.
    ///     If the save file exists, it replaces MapPath but everything else stays the same (station name and such).
    /// </summary>
    public static readonly CVarDef<string>
        PersistenceMap = CVarDef.Create("game.persistencemap", "Empty", CVar.ARCHIVE);

    /// <summary>
    ///     Prototype to use for map pool.
    /// </summary>
    public static readonly CVarDef<string>
        GameMapPool = CVarDef.Create("game.map_pool", "DefaultMapPool", CVar.SERVERONLY);

    /// <summary>
    /// The depth of the queue used to calculate which map is next in rotation.
    /// This is how long the game "remembers" that some map was put in play. Default is 16 rounds.
    /// </summary>
    public static readonly CVarDef<int>
        GameMapMemoryDepth = CVarDef.Create("game.map_memory_depth", 16, CVar.SERVERONLY);

    /// <summary>
    /// Is map rotation enabled?
    /// </summary>
    public static readonly CVarDef<bool>
        GameMapRotation = CVarDef.Create("game.map_rotation", true, CVar.SERVERONLY);

    /// <summary>
    /// If roles should be restricted based on time.
    /// </summary>
    public static readonly CVarDef<bool>
        GameRoleTimers = CVarDef.Create("game.role_timers", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Whether or not disconnecting inside of a cryopod should remove the character or just store them until they reconnect.
    /// </summary>
    public static readonly CVarDef<bool>
        GameCryoSleepRejoining = CVarDef.Create("game.cryo_sleep_rejoining", false, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     When enabled, guests will be assigned permanent UIDs and will have their preferences stored.
    /// </summary>
    public static readonly CVarDef<bool> GamePersistGuests =
        CVarDef.Create("game.persistguests", true, CVar.ARCHIVE | CVar.SERVERONLY);

    public static readonly CVarDef<bool> GameDiagonalMovement =
        CVarDef.Create("game.diagonalmovement", true, CVar.ARCHIVE);

    public static readonly CVarDef<int> SoftMaxPlayers =
        CVarDef.Create("game.soft_max_players", 30, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    /// If a player gets denied connection to the server,
    /// how long they are forced to wait before attempting to reconnect.
    /// </summary>
    public static readonly CVarDef<int> GameServerFullReconnectDelay =
        CVarDef.Create("game.server_full_reconnect_delay", 30, CVar.SERVERONLY);

    /// <summary>
    /// Whether or not panic bunker is currently enabled.
    /// </summary>
    public static readonly CVarDef<bool> PanicBunkerEnabled =
        CVarDef.Create("game.panic_bunker.enabled", false, CVar.NOTIFY | CVar.REPLICATED);

    /// <summary>
    /// Whether or not the panic bunker will disable when an admin comes online.
    /// </summary>
    public static readonly CVarDef<bool> PanicBunkerDisableWithAdmins =
        CVarDef.Create("game.panic_bunker.disable_with_admins", false, CVar.SERVERONLY);

    /// <summary>
    /// Whether or not the panic bunker will enable when no admins are online.
    /// </summary>
    public static readonly CVarDef<bool> PanicBunkerEnableWithoutAdmins =
        CVarDef.Create("game.panic_bunker.enable_without_admins", false, CVar.SERVERONLY);

    /// <summary>
    /// Whether or not the panic bunker will count deadminned admins for
    /// <see cref="PanicBunkerDisableWithAdmins"/> and
    /// <see cref="PanicBunkerEnableWithoutAdmins"/>
    /// </summary>
    public static readonly CVarDef<bool> PanicBunkerCountDeadminnedAdmins =
        CVarDef.Create("game.panic_bunker.count_deadminned_admins", false, CVar.SERVERONLY);

    /// <summary>
    /// Show reason of disconnect for user or not.
    /// </summary>
    public static readonly CVarDef<bool> PanicBunkerShowReason =
        CVarDef.Create("game.panic_bunker.show_reason", false, CVar.SERVERONLY);

    /// <summary>
    /// Minimum age of the account (from server's PoV, so from first-seen date) in hours.
    /// </summary>
    public static readonly CVarDef<int> PanicBunkerMinAccountAge =
        CVarDef.Create("game.panic_bunker.min_account_age", 24, CVar.SERVERONLY);

    /// <summary>
    /// Minimal overall played time.
    /// </summary>
    public static readonly CVarDef<int> PanicBunkerMinOverallHours =
        CVarDef.Create("game.panic_bunker.min_overall_hours", 10, CVar.SERVERONLY);

    /// <summary>
    /// A custom message that will be used for connections denied to the panic bunker
    /// If not empty, then will overwrite <see cref="PanicBunkerShowReason"/>
    /// </summary>
    public static readonly CVarDef<string> PanicBunkerCustomReason =
        CVarDef.Create("game.panic_bunker.custom_reason", string.Empty, CVar.SERVERONLY);

    /// <summary>
    /// Allow bypassing the panic bunker if the user is whitelisted.
    /// </summary>
    public static readonly CVarDef<bool> BypassBunkerWhitelist =
        CVarDef.Create("game.panic_bunker.whitelisted_can_bypass", true, CVar.SERVERONLY);

    /*
        * TODO: Remove baby jail code once a more mature gateway process is established. This code is only being issued as a stopgap to help with potential tiding in the immediate future.
        */

    /// <summary>
    /// Whether the baby jail is currently enabled.
    /// </summary>
    public static readonly CVarDef<bool> BabyJailEnabled  =
        CVarDef.Create("game.baby_jail.enabled", false, CVar.NOTIFY | CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    /// Show reason of disconnect for user or not.
    /// </summary>
    public static readonly CVarDef<bool> BabyJailShowReason =
        CVarDef.Create("game.baby_jail.show_reason", false, CVar.SERVERONLY);

    /// <summary>
    /// Maximum age of the account (from server's PoV, so from first-seen date) in hours that can access baby
    /// jailed servers.
    /// </summary>
    public static readonly CVarDef<int> BabyJailMaxAccountAge =
        CVarDef.Create("game.baby_jail.max_account_age", 24, CVar.SERVERONLY);

    /// <summary>
    /// Maximum overall played time allowed to access baby jailed servers.
    /// </summary>
    public static readonly CVarDef<int> BabyJailMaxOverallHours =
        CVarDef.Create("game.baby_jail.max_overall_hours", 2, CVar.SERVERONLY);

    /// <summary>
    /// A custom message that will be used for connections denied due to the baby jail.
    /// If not empty, then will overwrite <see cref="BabyJailShowReason"/>
    /// </summary>
    public static readonly CVarDef<string> BabyJailCustomReason =
        CVarDef.Create("game.baby_jail.custom_reason", string.Empty, CVar.SERVERONLY);

    /// <summary>
    /// Allow bypassing the baby jail if the user is whitelisted.
    /// </summary>
    public static readonly CVarDef<bool> BypassBabyJailWhitelist =
        CVarDef.Create("game.baby_jail.whitelisted_can_bypass", true, CVar.SERVERONLY);

    /// <summary>
    /// Make people bonk when trying to climb certain objects like tables.
    /// </summary>
    public static readonly CVarDef<bool> GameTableBonk =
        CVarDef.Create("game.table_bonk", false, CVar.REPLICATED);

    /// <summary>
    /// Whether or not status icons are rendered for everyone.
    /// </summary>
    public static readonly CVarDef<bool> GlobalStatusIconsEnabled =
        CVarDef.Create("game.global_status_icons_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Whether or not status icons are rendered on this specific client.
    /// </summary>
    public static readonly CVarDef<bool> LocalStatusIconsEnabled =
        CVarDef.Create("game.local_status_icons_enabled", true, CVar.CLIENTONLY);

    /// <summary>
    /// Whether or not coordinates on the Debug overlay should only be available to admins.
    /// </summary>
    public static readonly CVarDef<bool> DebugCoordinatesAdminOnly =
        CVarDef.Create("game.debug_coordinates_admin_only", true, CVar.SERVER | CVar.REPLICATED);


    /// <summary>
    ///     Whether to allow characters to select traits.
    /// </summary>
    public static readonly CVarDef<bool> GameTraitsEnabled =
        CVarDef.Create("game.traits_enabled", true, CVar.REPLICATED);

    /// <summary>
    ///     How many traits a character can have at most.
    /// </summary>
    public static readonly CVarDef<int> GameTraitsMax =
        CVarDef.Create("game.traits_max", 14, CVar.REPLICATED);

    /// <summary>
    ///     How many points a character should start with.
    /// </summary>
    public static readonly CVarDef<int> GameTraitsDefaultPoints =
        CVarDef.Create("game.traits_default_points", 10, CVar.REPLICATED);

    /// <summary>
    ///     Whether the game will SMITE people who used cheat engine to spawn with all of the traits.
    ///     Illegal trait totals will still be logged even if this is disabled.
    ///     If you are intending to decrease the trait points availability, or modify the costs of traits, consider temporarily disabling this.
    /// </summary>
    public static readonly CVarDef<bool> TraitsPunishCheaters =
        CVarDef.Create("game.traits_punish_cheaters", false, CVar.REPLICATED);

    /// <summary>
    ///     Whether to allow characters to select loadout items.
    /// </summary>
    public static readonly CVarDef<bool> GameLoadoutsEnabled =
        CVarDef.Create("game.loadouts_enabled", true, CVar.REPLICATED);

    /// <summary>
    ///     How many points to give to each player for loadouts.
    /// </summary>
    public static readonly CVarDef<int> GameLoadoutsPoints =
        CVarDef.Create("game.loadouts_points", 14, CVar.REPLICATED);


    /// <summary>
    ///     Whether to repeat eating doafters after completion
    /// </summary>
    public static readonly CVarDef<bool> GameAutoEatFood =
        CVarDef.Create("game.auto_eat_food", false, CVar.REPLICATED);

    /// <summary>
    ///     Whether to repeat drinking doafters after completion
    /// </summary>
    public static readonly CVarDef<bool> GameAutoEatDrinks =
        CVarDef.Create("game.auto_eat_drinks", false, CVar.REPLICATED);

    /// <summary>
    ///     Whether item slots, such as power cell slots or AME fuel cell slots, should support quick swap if it is not otherwise specified in their YAML prototype.
    /// </summary>
    public static readonly CVarDef<bool> AllowSlotQuickSwap =
        CVarDef.Create("game.slot_quick_swap", false, CVar.REPLICATED);

#if EXCEPTION_TOLERANCE
    /// <summary>
    ///     Amount of times round start must fail before the server is shut down.
    ///     Set to 0 or a negative number to disable.
    /// </summary>
    public static readonly CVarDef<int> RoundStartFailShutdownCount =
        CVarDef.Create("game.round_start_fail_shutdown_count", 5, CVar.SERVERONLY | CVar.SERVER);
#endif

    /// <summary>
    /// Delay between station alert level changes.
    /// </summary>
    public static readonly CVarDef<int> GameAlertLevelChangeDelay =
        CVarDef.Create("game.alert_level_change_delay", 30, CVar.SERVERONLY);

    /// <summary>
    /// The time in seconds that the server should wait before restarting the round.
    /// Defaults to 2 minutes.
    /// </summary>
    public static readonly CVarDef<float> RoundRestartTime =
        CVarDef.Create("game.round_restart_time", 120f, CVar.SERVERONLY);

    /// <summary>
    /// The prototype to use for secret weights.
    /// </summary>
    public static readonly CVarDef<string> SecretWeightPrototype =
        CVarDef.Create("game.secret_weight_prototype", "Secret", CVar.SERVERONLY);

    /// <summary>
    /// The id of the sound collection to randomly choose a sound from and play when the round ends.
    /// </summary>
    public static readonly CVarDef<string> RoundEndSoundCollection =
        CVarDef.Create("game.round_end_sound_collection", "RoundEnd", CVar.SERVERONLY);

    /// <summary>
    /// Set to true to enable the dynamic hostname system.
    /// Automatically updates the hostname to include current map and preset.
    /// Configure what that looks like for you in Resources/Prototypes/Locale/en-US/dynamichostname/hostname.ftl
    /// </summary>
    public static readonly CVarDef<bool> UseDynamicHostname =
        CVarDef.Create("game.use_dynamic_hostname", false, CVar.SERVERONLY);
}
