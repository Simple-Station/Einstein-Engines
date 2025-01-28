using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Customization.Systems;
using Content.Shared.Database;
using Content.Shared.Players;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Content.Shared.Whitelist;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly AdminSystem _adminSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args) =>
        ApplyTraits(args.Mob, args.JobId, args.Profile,
            _playTimeTracking.GetTrackerTimes(args.Player), args.Player.ContentData()?.Whitelisted ?? false);

    /// <summary>
    ///     Adds the traits selected by a player to an entity.
    /// </summary>
    public void ApplyTraits(EntityUid uid, ProtoId<JobPrototype>? jobId, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, bool punishCheater = true)
    {
        var pointsTotal = _configuration.GetCVar(CCVars.GameTraitsDefaultPoints);
        var traitSelections = _configuration.GetCVar(CCVars.GameTraitsMax);
        if (jobId is not null && !_prototype.TryIndex(jobId, out var jobPrototype)
            && jobPrototype is not null && !jobPrototype.ApplyTraits)
            return;

        var jobPrototypeToUse = _prototype.Index(jobId ?? _prototype.EnumeratePrototypes<JobPrototype>().First().ID);

        foreach (var traitId in profile.TraitPreferences)
        {
            if (!_prototype.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                DebugTools.Assert($"No trait found with ID {traitId}!");
                return;
            }

            if (!_characterRequirements.CheckRequirementsValid(
                traitPrototype.Requirements,
                jobPrototypeToUse,
                profile, playTimes, whitelisted, traitPrototype,
                EntityManager, _prototype, _configuration,
                out _))
                continue;

            // To check for cheaters. :FaridaBirb.png:
            pointsTotal += traitPrototype.Points;
            --traitSelections;

            AddTrait(uid, traitPrototype);
        }

        if (punishCheater && (pointsTotal < 0 || traitSelections < 0))
            PunishCheater(uid);
    }

    /// <summary>
    ///     Adds a single Trait Prototype to an Entity.
    /// </summary>
    public void AddTrait(EntityUid uid, TraitPrototype traitPrototype)
    {
        foreach (var function in traitPrototype.Functions)
            function.OnPlayerSpawn(uid, _componentFactory, EntityManager, _serialization);
    }

    /// <summary>
    ///     On a non-cheating client, it's not possible to save a character with a negative number of traits. This can however
    ///     trigger incorrectly if a character was saved, and then at a later point in time an admin changes the traits Cvars to reduce the points.
    ///     Or if the points costs of traits is increased.
    /// </summary>
    private void PunishCheater(EntityUid uid)
    {
        _adminLog.Add(LogType.AdminMessage, LogImpact.High,
            $"{ToPrettyString(uid):entity} attempted to spawn with an invalid trait list. This might be a mistake, or they might be cheating");

        if (!_configuration.GetCVar(CCVars.TraitsPunishCheaters)
            || !_playerManager.TryGetSessionByEntity(uid, out var targetPlayer))
            return;

        // For maximum comedic effect, this is plenty of time for the cheater to get on station and start interacting with people.
        var timeToDestroy = _random.NextFloat(120, 360);

        Timer.Spawn(TimeSpan.FromSeconds(timeToDestroy), () => VaporizeCheater(targetPlayer));
    }

    /// <summary>
    ///     https://www.youtube.com/watch?v=X2QMN0a_TrA
    /// </summary>
    private void VaporizeCheater (Robust.Shared.Player.ICommonSession targetPlayer)
    {
        _adminSystem.Erase(targetPlayer);

        var feedbackMessage = $"[font size=24][color=#ff0000]{"You have spawned in with an illegal trait point total. If this was a result of cheats, then your nonexistence is a skill issue. Otherwise, feel free to click 'Return To Lobby', and fix your trait selections."}[/color][/font]";
        _chatManager.ChatMessageToOne(
            ChatChannel.Emotes,
            feedbackMessage,
            feedbackMessage,
            EntityUid.Invalid,
            false,
            targetPlayer.Channel);
    }
}
