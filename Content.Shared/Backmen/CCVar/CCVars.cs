using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.Backmen.CCVar;

// ReSharper disable once InconsistentNaming
[CVarDefs]
public sealed class CCVars
{
    /*
     * GPT
     */
    public static readonly CVarDef<bool>
        GptEnabled = CVarDef.Create("gpt.enabled", false, CVar.SERVERONLY);

    public static readonly CVarDef<string>
        GptModel = CVarDef.Create("gpt.model", "gpt-3.5-turbo-0613", CVar.SERVERONLY);

    public static readonly CVarDef<string>
        GptApiUrl = CVarDef.Create("gpt.api", "https://api.openai.com/v1/", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string>
        GptApiToken = CVarDef.Create("gpt.token", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /*
     * Queue
     */

    /// <summary>
    ///     Controls if the connections queue is enabled. If enabled stop kicking new players after `SoftMaxPlayers` cap and instead add them to queue.
    /// </summary>
    public static readonly CVarDef<bool>
        QueueEnabled = CVarDef.Create("queue.enabled", false, CVar.SERVERONLY);

    public static readonly CVarDef<bool>
        QueueAltEnabled = CVarDef.Create("queue.alt_servers", false, CVar.SERVERONLY);

    /*
     * Discord Auth
     */

    /// <summary>
    ///     Enabled Discord linking, show linking button and modal window
    /// </summary>
    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("discord_auth.enabled", false, CVar.SERVERONLY);

    /// <summary>
    ///     URL of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiUrl =
        CVarDef.Create("discord_auth.api_url", "", CVar.SERVERONLY);

    /// <summary>
    ///     Secret key of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiKey =
        CVarDef.Create("discord_auth.api_key", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /**
     * Sponsors
     */

    /// <summary>
    ///     URL of the sponsors server API.
    /// </summary>
    public static readonly CVarDef<string> SponsorsApiUrl =
        CVarDef.Create("sponsor.api_url", "", CVar.SERVERONLY);

    public static readonly CVarDef<string> SponsorsSelectedGhost =
        CVarDef.Create("sponsor.ghost", "", CVar.REPLICATED | CVar.CLIENT);


    public static readonly CVarDef<bool>
        EconomyWagesEnabled = CVarDef.Create("economy.wages_enabled", true, CVar.SERVERONLY);

    public static readonly CVarDef<bool>
        WhitelistRolesEnabled = CVarDef.Create("game.whitelist_role_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Whether the Shipyard is enabled.
    /// </summary>
    public static readonly CVarDef<bool> Shipyard =
        CVarDef.Create("shuttle.shipyard", true, CVar.SERVERONLY);

    /*
     * Glimmer
     */

    /// <summary>
    ///    Whether glimmer is enabled.
    /// </summary>
    public static readonly CVarDef<bool> GlimmerEnabled =
        CVarDef.Create("glimmer.enabled", true, CVar.REPLICATED);

    /// <summary>
    ///     Passive glimmer drain per second.
    ///     Note that this is randomized and this is an average value.
    /// </summary>
    public static readonly CVarDef<float> GlimmerLostPerSecond =
        CVarDef.Create("glimmer.passive_drain_per_second", 0.1f, CVar.SERVERONLY);

    /// <summary>
    ///     Whether random rolls for psionics are allowed.
    ///     Guaranteed psionics will still go through.
    /// </summary>
    public static readonly CVarDef<bool> PsionicRollsEnabled =
        CVarDef.Create("psionics.rolls_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// Shipwrecked
    /// </summary>
    public static readonly CVarDef<int> ShipwreckedMaxPlayers =
        CVarDef.Create("shipwrecked.max_players", 15);

    /// <summary>
    /// Damage
    /// </summary>
    public static readonly CVarDef<float> DamageVariance =
        CVarDef.Create("damage.variance", 0.15f, CVar.SERVER | CVar.REPLICATED);
    /*
 * FleshCult
 */

    public static readonly CVarDef<int> FleshCultMinPlayers =
        CVarDef.Create("fleshcult.min_players", 25, CVar.SERVERONLY);

    public static readonly CVarDef<int> FleshCultMaxCultist =
        CVarDef.Create("fleshcult.max_cultist", 6, CVar.SERVERONLY);

    public static readonly CVarDef<int> FleshCultPlayersPerCultist =
        CVarDef.Create("fleshcult.players_per_cultist", 7, CVar.SERVERONLY);

    /*
     * bloodsucker
     */

    public static readonly CVarDef<int> BloodsuckerMaxPerBloodsucker =
        CVarDef.Create("bloodsucker.max", 5, CVar.SERVERONLY);

    public static readonly CVarDef<int> BloodsuckerPlayersPerBloodsucker =
        CVarDef.Create("bloodsucker.players_per", 10, CVar.SERVERONLY);

    /*
     * Blob
     */

    public static readonly CVarDef<int> BlobMax =
        CVarDef.Create("blob.max", 3, CVar.SERVERONLY);

    public static readonly CVarDef<int> BlobPlayersPer =
        CVarDef.Create("blob.players_per", 20, CVar.SERVERONLY);
}
