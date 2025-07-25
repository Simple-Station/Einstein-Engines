using Content.Server.Announcements.Systems;
using Content.Server.Anomaly;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Player;

namespace Content.Server.StationEvents.Events;

public sealed class AnomalySpawnRule : StationEventSystem<AnomalySpawnRuleComponent>
{
    [Dependency] private readonly AnomalySystem _anomaly = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    protected override void Added(EntityUid uid, AnomalySpawnRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        _announcer.SendAnnouncement(
            _announcer.GetAnnouncementId(args.RuleId),
            "anomaly-spawn-event-announcement",
            colorOverride: stationEvent.StartAnnouncementColor,
            localeArgs: [("sighting", Loc.GetString($"anomaly-spawn-sighting-{RobustRandom.Next(1, 6)}")), ]
        );
    }

    protected override void Started(EntityUid uid, AnomalySpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        if (!TryComp<StationDataComponent>(chosenStation, out var stationData))
            return;

        var grid = StationSystem.GetLargestGrid(stationData);

        if (grid is null)
            return;

        var amountToSpawn = 1;
        for (var i = 0; i < amountToSpawn; i++)
        {
            _anomaly.SpawnOnRandomGridLocation(grid.Value, component.AnomalySpawnerPrototype);
        }
    }
}
