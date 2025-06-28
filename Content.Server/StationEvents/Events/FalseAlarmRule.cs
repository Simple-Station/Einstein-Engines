using System.Linq;
using Content.Server.Announcements.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class FalseAlarmRule : StationEventSystem<FalseAlarmRuleComponent>
{
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    protected override void Started(EntityUid uid, FalseAlarmRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var allEv = _event.AllEvents().Where(p => p.Value.StartAnnouncement).ToList();
        var picked = RobustRandom.Pick(allEv);

        _announcer.SendAnnouncement(
            _announcer.GetAnnouncementId(picked.Key.ID),
            _announcer.GetEventLocaleString(_announcer.GetAnnouncementId(picked.Key.ID)),
            colorOverride: picked.Value.StartAnnouncementColor,
            //TODO This isn't a good solution, but I can't think of something better
            localeArgs: [("data", Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}")), ]
        );
    }
}
