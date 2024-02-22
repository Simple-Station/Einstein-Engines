using System.Linq;
using Content.Server.Antag;
using Content.Server.Backmen.Vampiric;
using Content.Server.Bible.Components;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Shared.Backmen.Blob;
using Content.Shared.Backmen.CCVar;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Backmen.Blob.Rule;

public sealed class BlobGameRuleSystem : GameRuleSystem<BlobGameRuleComponent>
{
    private ISawmill _sawmill = default!;


    private int PlayersPerBlob => _cfg.GetCVar(CCVars.BlobPlayersPer);
    private int MaxBlob => _cfg.GetCVar(CCVars.BlobMax);

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("preset");

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnPlayersSpawned);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(HandleLatejoin);
    }

    private void HandleLatejoin(PlayerSpawnCompleteEvent ev)
    {
        var query = EntityQueryEnumerator<BlobGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var blob, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            if (blob.TotalBlobs >= MaxBlob)
                continue;

            if (!ev.LateJoin)
                continue;

            if (!ev.Profile.AntagPreferences.Contains(Blob))
                continue;

            if (ev.JobId == null || !_prototypeManager.TryIndex<JobPrototype>(ev.JobId, out var job))
                continue;

            if (!job.CanBeAntag)
                continue;

            // the nth player we adjust our probabilities around
            var target = PlayersPerBlob * blob.TotalBlobs + 1;

            var chance = 1f / PlayersPerBlob;

            // If we have too many traitors, divide by how many players below target for next traitor we are.
            if (ev.JoinOrder < target)
            {
                chance /= (target - ev.JoinOrder);
            }
            else // Tick up towards 100% chance.
            {
                chance *= ((ev.JoinOrder + 1) - target);
            }

            if (chance > 1)
                chance = 1;

            // Now that we've calculated our chance, roll and make them a traitor if we roll under.
            // You get one shot.
            if (_random.Prob(chance) && ev.Player.AttachedEntity.HasValue)
            {
                MakeBlob(blob, ev.Player);
            }
        }
    }

    private void MakeBlob(BlobGameRuleComponent blob, ICommonSession player)
    {
        if (!player.AttachedEntity.HasValue)
            return;

        blob.TotalBlobs++;
        EnsureComp<BlobCarrierComponent>(player.AttachedEntity.Value).HasMind = true;
    }

    private void OnPlayersSpawned(RulePlayerJobsAssignedEvent ev)
    {
        var query = EntityQueryEnumerator<BlobGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var blob, out var gameRule))
        {
            var plr = new Dictionary<ICommonSession, HumanoidCharacterProfile>();

            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            foreach (var player in ev.Players)
            {
                if (!ev.Profiles.ContainsKey(player.UserId))
                    continue;

                plr.Add(player, ev.Profiles[player.UserId]);
            }

            DoBlobStart(blob, plr);
        }
    }

    private void DoBlobStart(BlobGameRuleComponent blob,
        Dictionary<ICommonSession, HumanoidCharacterProfile> startCandidates)
    {
        var numTraitors = MathHelper.Clamp(startCandidates.Count / PlayersPerBlob, 1, MaxBlob);
        var traitorPool = _antagSelection.FindPotentialAntags(startCandidates, Blob);
        var selectedTraitors = _antagSelection.PickAntag(numTraitors, traitorPool);

        foreach (var traitor in selectedTraitors)
        {
            if (!traitor.AttachedEntity.HasValue)
                continue;

            MakeBlob(blob, traitor);
        }
    }

    private void OnStartAttempt(RoundStartAttemptEvent ev)
    {
        var query = EntityQueryEnumerator<BlobGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out _, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            if (ev.Players.Length == 0)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("blob-no-one-ready"));
                ev.Cancel();
            }
        }
    }

    [ValidatePrototypeId<AntagPrototype>]
    private const string Blob = "Blob";
}
