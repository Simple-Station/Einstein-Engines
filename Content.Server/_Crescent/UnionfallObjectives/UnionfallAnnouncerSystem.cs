using Content.Shared._Crescent.ShipShields;
using Robust.Shared.Physics.Systems;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Content.Server.Power.Components;
using Content.Server._Crescent.UnionfallCapturePoint;
using Content.Shared.Interaction;
using Content.Shared.Preferences;
using Content.Server.Preferences.Managers;
using Robust.Shared.Network;
using Content.Shared._Crescent.HullrotFaction;
using Robust.Shared.Player;
using Content.Server.Announcements.Systems;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.DoAfter;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Content.Shared._Crescent.UnionfallCapturePoint;
using Robust.Shared.Timing;


namespace Content.Server._Crescent.UnionfallCapturePoint;

public sealed class UnionfallAnnouncerSystem : EntitySystem
{

    [Dependency] private readonly AnnouncerSystem _announcer = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<UnionfallAnnouncerComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, UnionfallAnnouncerComponent component, ComponentInit args)
    {
        TimeSpan graceTime = TimeSpan.FromSeconds(component.GracePeriod);
        Timer.Spawn(TimeSpan.FromMinutes(1), () => AnnouncementWarStart(graceTime));
        Timer.Spawn(graceTime * 0.25, () => AnnouncementWarPeriodic(graceTime - graceTime * 0.25));
        Timer.Spawn(graceTime * 0.50, () => AnnouncementWarPeriodic(graceTime - graceTime * 0.50));
        Timer.Spawn(graceTime * 0.75, () => AnnouncementWarPeriodic(graceTime - graceTime * 0.75));
        Timer.Spawn(graceTime - TimeSpan.FromMinutes(1), AnnouncementWarAlmost);
        Timer.Spawn(graceTime, AnnouncementWarGraceOver); // TODO: turn this into a cool 10 second countdown
    }


    private void AnnouncementWarStart(TimeSpan time)
    {
        _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallBegin"), Filter.Broadcast(),
                "HADAL STORM DETECTED - Emergency repulsion field deployed, estimated storm dispersion time: <" + time + ">...  Dispersion pattern confirms presence of a hostile fleet in the operating area.");
    }
    private void AnnouncementWarPeriodic(TimeSpan time)
    {
        _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallPeriodic"), Filter.Broadcast(),
                "<" + time + "> until the Hadal storm disperses.");
    }

    private void AnnouncementWarAlmost()
    {
        _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallAlmost"), Filter.Broadcast(),
                "<00:01:00> LEFT UNTIL FULL HADAL STORM DISPERSION.");
    }
    private void AnnouncementWarGraceOver()
    {
        _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallGraceOver"), Filter.Broadcast(),
                "HADAL STORM HAS DISPERSED. Emergency dispersion field has been disabled. Long-Range radar readings confirm presence of hostile fleet.");
    }
}
