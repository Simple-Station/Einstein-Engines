using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.AlertLevel;
using Content.Server.Antag;
using Content.Server._Goobstation.Blob;
using Content.Server._Goobstation.Blob.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Nuke;
using Content.Server.Objectives;
using Content.Server.RoundEnd;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Objectives.Components;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Content.Server.Announcements.Systems;

namespace Content.Server.GameTicking.Rules;

public sealed class BlobRuleSystem : GameRuleSystem<BlobRuleComponent>
{
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly NukeCodePaperSystem _nukeCode = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly ObjectivesSystem _objectivesSystem = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);
    }

    protected override void Started(EntityUid uid, BlobRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        var activeRules = QueryActiveRules();
        while (activeRules.MoveNext(out var entityUid, out _, out _, out _))
        {
            if (uid == entityUid)
                continue;

            GameTicker.EndGameRule(uid, gameRule);
            Log.Warning("blob is active!!! remove!");
            break;
        }
    }

    protected override void ActiveTick(EntityUid uid, BlobRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        component.Accumulator += frameTime;

        if (component.Accumulator < 10)
            return;

        component.Accumulator = 0;

        var check = new Dictionary<EntityUid, long>();
        var blobCoreQuery = EntityQueryEnumerator<BlobCoreComponent, MetaDataComponent, TransformComponent>();
        while (blobCoreQuery.MoveNext(out var ent, out var comp, out var md, out var xform))
        {
            if (TerminatingOrDeleted(ent, md) ||
                !CheckBlobInStation(ent, xform, out var stationUid))
            {
                continue;
            }

            check.TryAdd(stationUid.Value, 0);

            check[stationUid.Value] += comp.BlobTiles.Count;
        }

        foreach (var (station, length) in check.AsParallel())
        {
            CheckChangeStage(station, component, length);
        }
    }

    private bool CheckBlobInStation(EntityUid blobCore, TransformComponent? xform, [NotNullWhen(true)] out EntityUid? stationUid)
    {
        var station = _stationSystem.GetOwningStation(blobCore, xform);
        if (station == null || !HasComp<StationEventEligibleComponent>(station.Value))
        {
            _chatManager.SendAdminAlert(blobCore, Loc.GetString("blob-alert-out-off-station"));
            QueueDel(blobCore);
            stationUid = null;
            return false;
        }

        stationUid = station.Value;
        return true;
    }

    private const string StationAlertCritical = "delta";
    private const string StationAlertDetected = "red";

    private void CheckChangeStage(
        Entity<StationBlobConfigComponent?> stationUid,
        BlobRuleComponent blobRuleComp,
        long blobTilesCount)
    {
        Resolve(stationUid, ref stationUid.Comp, false);

        if (blobTilesCount >= (stationUid.Comp?.StageBegin ?? StationBlobConfigComponent.DefaultStageBegin)
            && _roundEndSystem.ExpectedCountdownEnd != null)
        {
            _roundEndSystem.CancelRoundEndCountdown(checkCooldown: false);

            _announcer.SendAnnouncement(
                "blob-recall-shuttle",
                Filter.Broadcast(),
                Loc.GetString("blob-alert-recall-shuttle"),
                station: stationUid,
                colorOverride: Color.Red
                );
        }

        switch (blobRuleComp.Stage)
        {
            case BlobStage.Default when blobTilesCount >= (stationUid.Comp?.StageBegin ?? StationBlobConfigComponent.DefaultStageBegin):
                blobRuleComp.Stage = BlobStage.Begin;

                _announcer.SendAnnouncement(
                    "blob-detect",
                    Filter.Broadcast(),
                    Loc.GetString("blob-alert-detect"),
                    station: stationUid,
                    colorOverride: Color.Red
                    );

                // blobRuleComp.DetectedAudio,

                _alertLevelSystem.SetLevel(stationUid, StationAlertDetected, true, true, true, true);

                RaiseLocalEvent(stationUid,
                    new BlobChangeLevelEvent
                    {
                        Station = stationUid,
                        Level = blobRuleComp.Stage
                    },
                    broadcast: true);
                return;
            case BlobStage.Begin when blobTilesCount >= (stationUid.Comp?.StageCritical ?? StationBlobConfigComponent.DefaultStageCritical):
                {
                    if (_nukeCode.SendNukeCodes(stationUid))//send the nuke code?
                    {
                        blobRuleComp.Stage = BlobStage.Critical;

                        _announcer.SendAnnouncement(
                            "blob-critical",
                            Filter.Broadcast(),
                            Loc.GetString("blob-alert-critical"),
                            station: stationUid,
                            colorOverride: Color.Red
                            );
                        // blobRuleComp.CriticalAudio
                    }
                    else
                    {
                        blobRuleComp.Stage = BlobStage.Critical;

                        _announcer.SendAnnouncement(
                            "blob-critical-no-nuke",
                            Filter.Broadcast(),
                            Loc.GetString("blob-alert-critical-NoNukeCode"),
                            colorOverride: Color.Red
                            );
                        // blobRuleComp.CriticalAudio
                    }

                    _alertLevelSystem.SetLevel(stationUid, StationAlertCritical, true, true, true, true);

                    RaiseLocalEvent(stationUid,
                        new BlobChangeLevelEvent
                        {
                            Station = stationUid,
                            Level = blobRuleComp.Stage
                        },
                        broadcast: true);
                    return;
                }
            case BlobStage.Critical when blobTilesCount >= (stationUid.Comp?.StageTheEnd ?? StationBlobConfigComponent.DefaultStageEnd):
                {
                    blobRuleComp.Stage = BlobStage.TheEnd;
                    _roundEndSystem.EndRound();

                    RaiseLocalEvent(stationUid,
                        new BlobChangeLevelEvent
                        {
                            Station = stationUid,
                            Level = blobRuleComp.Stage
                        },
                        broadcast: true);
                    return;
                }
        }
    }

    private const string BlobIssuer = "objective-issuer-blob";

    protected override void AppendRoundEndText(
        EntityUid uid,
        BlobRuleComponent blob,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent ev)
    {
        if (blob.Blobs.Count < 1)
            return; // no blob no fun

        var result = Loc.GetString("blob-round-end-result", ("blobCount", blob.Blobs.Count));
        var totalPercentage = 0f;

        // Get the total amount of blob tiles
        foreach (var (mindId, mind) in blob.Blobs)
        {
            var objectives = mind.Objectives.ToArray();

            foreach (var objective in objectives)
            {
                var comp = Comp<ObjectiveComponent>(objective);
                if (comp.Issuer != BlobIssuer)
                    continue;

                var info = _objectivesSystem.GetInfo(objective, mindId, mind);
                totalPercentage += info?.Progress ?? 0;
            }
        }

        if (totalPercentage >= 0.99f)
        {
            result += "\n" + Loc.GetString("blob-end-victory");
        }
        else
        {
            result += "\n" + Loc.GetString("blob-end-fail");
            result += "\n" + Loc.GetString("blob-end-fail-progress", ("progress", (int) (totalPercentage * 100)));
        }

        result += "\n";

        // yeah this is duplicated from traitor rules lol, there needs to be a generic rewrite where it just goes through all minds with objectives
        foreach (var (mindId, mind) in blob.Blobs)
        {
            var name = mind.CharacterName;
            _mindSystem.TryGetSession(mindId, out var session);
            var username = session?.Name;

            var objectives = mind.Objectives.ToArray();
            if (objectives.Length == 0)
            {
                if (username != null)
                {
                    if (name == null)
                        result += "\n" + Loc.GetString("blob-user-was-a-blob", ("user", username));
                    else
                    {
                        result += "\n" + Loc.GetString("blob-user-was-a-blob-named",
                            ("user", username),
                            ("name", name));
                    }
                }
                else if (name != null)
                    result += "\n" + Loc.GetString("blob-was-a-blob-named", ("name", name));

                continue;
            }

            if (username != null)
            {
                if (name == null)
                {
                    result += "\n" + Loc.GetString("blob-user-was-a-blob-with-objectives",
                        ("user", username));
                }
                else
                {
                    result += "\n" + Loc.GetString("blob-user-was-a-blob-with-objectives-named",
                        ("user", username),
                        ("name", name));
                }
            }
            else if (name != null)
                result += "\n" + Loc.GetString("blob-was-a-blob-with-objectives-named", ("name", name));

            foreach (var objectiveGroup in objectives.GroupBy(o => Comp<ObjectiveComponent>(o).Issuer == BlobIssuer))
            {
                if (!objectiveGroup.Key)
                    continue;

                foreach (var objective in objectiveGroup)
                {

                    var info = _objectivesSystem.GetInfo(objective, mindId, mind);
                    if (info == null)
                        continue;

                    var progress = info.Value.Progress;

                    result += "\n- " + Loc.GetString(
                        "blob-objective-percentage",
                        ("progress", (int) (progress * 100))
                    );

                }
            }
        }

        ev.AddLine(result);
    }

    public void MakeBlob(EntityUid player)
    {
        var comp = EnsureComp<BlobCarrierComponent>(player);
        comp.HasMind = HasComp<ActorComponent>(player);
        comp.TransformationDelay = 10 * 60; // 10min
    }

    private void AfterAntagSelected(EntityUid uid, BlobRuleComponent component, AfterAntagEntitySelectedEvent args)
    {
        MakeBlob(args.EntityUid);
    }
}
