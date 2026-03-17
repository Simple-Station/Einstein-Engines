using Content.Server.Administration.Logs;
using Content.Shared._DV.CustomObjectiveSummary;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Robust.Shared.Network;
using Robust.Server.Player;

namespace Content.Server._DV.CustomObjectiveSummary;

public sealed class CustomObjectiveSummarySystem : EntitySystem
{
    [Dependency] private readonly IServerNetManager _net = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EvacShuttleLeftEvent>(OnEvacShuttleLeft);
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);

        _net.RegisterNetMessage<CustomObjectiveClientSetObjective>(OnCustomObjectiveFeedback);
    }

    private void OnCustomObjectiveFeedback(CustomObjectiveClientSetObjective msg)
    {
        if (!_mind.TryGetMind(msg.MsgChannel.UserId, out var mind))
            return;

        if (mind.Value.Comp.Objectives.Count == 0)
            return;

        var comp = EnsureComp<CustomObjectiveSummaryComponent>(mind.Value);

        comp.ObjectiveSummary = msg.Summary;
        Dirty(mind.Value.Owner, comp);

        _adminLog.Add(LogType.Action, $"{ToPrettyString(mind.Value.Comp.OwnedEntity)} wrote objective summery: {msg.Summary}");
    }

    private void OnEvacShuttleLeft(EvacShuttleLeftEvent args)
    {
        var allMinds = _mind.GetAliveHumans();

        foreach (var mind in allMinds)
        {
            // Only send the popup to people with objectives.
            if (mind.Comp.Objectives.Count == 0)
                continue;

            if (mind.Comp.UserId == null || !_playerManager.TryGetSessionById(mind.Comp.UserId.Value, out var session))
                continue;

            RaiseNetworkEvent(new CustomObjectiveSummaryOpenMessage(), session);
        }
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        /* Orehum - No feedback no fun
        var allMinds = _mind.GetAliveHumans();

        foreach (var mind in allMinds)
        {
            if (mind.Comp.Objectives.Count == 0)
                continue;

            _feedback.SendPopupMind(mind, "RemoveGreentextPopup");
        }
        */
    }
}
