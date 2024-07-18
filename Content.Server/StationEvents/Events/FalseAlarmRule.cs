using System.Linq;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Content.Server.Announcements.Systems;

namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class FalseAlarmRule : StationEventSystem<FalseAlarmRuleComponent>
{
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    protected override void Started(EntityUid uid, FalseAlarmRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var allEv = _event.AllEvents().Select(p => p.Key).ToList();
        var picked = RobustRandom.Pick(allEv);

        _announcer.SendAnnouncement(
            _announcer.GetAnnouncementId(picked.ID),
            Filter.Broadcast(),
            _announcer.GetEventLocaleString(_announcer.GetAnnouncementId(picked.ID)),
            colorOverride: Color.Gold
        );
    }
}
