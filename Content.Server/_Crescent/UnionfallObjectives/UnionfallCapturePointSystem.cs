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

public sealed class UnionfallCapturePointSystem : EntitySystem
{

    [Dependency] private readonly AnnouncerSystem _announcer = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<UnionfallCapturePointComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<UnionfallCapturePointComponent, ActivateInWorldEvent>(OnActivatedInWorld);
        SubscribeLocalEvent<UnionfallCapturePointComponent, UnionfallCapturePointDoAfterEvent>(OnCaptureDoAfter);
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("audio.ambience");
    }

    private void OnComponentInit(EntityUid uid, UnionfallCapturePointComponent component, ComponentInit args)
    {
        TimeSpan graceTime = TimeSpan.FromSeconds(component.GracePeriod);
        Timer.Spawn(TimeSpan.FromMinutes(1), () => AnnouncementWarStart(graceTime));
        Timer.Spawn(graceTime * 0.25, () => AnnouncementWarPeriodic(graceTime - graceTime * 0.25));
        Timer.Spawn(graceTime * 0.50, () => AnnouncementWarPeriodic(graceTime - graceTime * 0.50));
        Timer.Spawn(graceTime * 0.75, () => AnnouncementWarPeriodic(graceTime - graceTime * 0.75));
        Timer.Spawn(graceTime - TimeSpan.FromMinutes(1), AnnouncementWarAlmost);
        Timer.Spawn(graceTime, AnnouncementWarGraceOver); // TODO: turn this into a cool 10 second countdown
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<UnionfallCapturePointComponent>();
        while (query.MoveNext(out var uid, out var capturepoint))
        {
            capturepoint.GracePeriod -= frameTime; //we do it this way so we can VVedit in admin mode midgame

            if (capturepoint.GracePeriod > 0f) //point is still in grace period
                return;

            if (capturepoint.CapturingFaction == null) //if nobody's capping it then don't do anything
                return;
            else //someone is capping it rn
            {
                capturepoint.CurrentCaptureProgress -= frameTime; //this is how the timer decreases
            }

            if (capturepoint.CurrentCaptureProgress <= 0) //capturing complete. TODO: NEED TO END THE ROUND SOMEHOW
            {
                _announcer.SendAnnouncement(_announcer.GetAnnouncementId("Fallback"), Filter.Broadcast(),
            capturepoint.CapturingFaction + " has secured the control point! The round is over.");
                _gameTicker.EndRound(capturepoint.CapturingFaction + " WON. RESTARTING ROUND IN 1 MINUTE");
                capturepoint.CurrentCaptureProgress = 999999;
                Timer.Spawn(TimeSpan.FromMinutes(1), _gameTicker.RestartRound);
            }
        }
    }

    private void OnActivatedInWorld(EntityUid uid, UnionfallCapturePointComponent component, ActivateInWorldEvent args)
    {
        if (component.GracePeriod > 0) //grace period still active
        {
            _popup.PopupEntity(Loc.GetString("capturepoint-grace-period-fail"), uid, args.User);
            return;
        }

        if (!TryComp<HullrotFactionComponent>(args.User, out var comp)) //someone with no faction interacted with this. modified client only
            return;
        string faction = comp.Faction;

        if (component.CapturingFaction == faction)
        {
            _popup.PopupEntity(Loc.GetString("capturepoint-same-faction-fail"), uid, args.User);
            return;
        }

        DoAfterArgs doAfterArguments = new DoAfterArgs(EntityManager, args.User, component.TimeToCapture, new UnionfallCapturePointDoAfterEvent(), uid, uid, null)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArguments, null);
    }

    private void OnCaptureDoAfter(EntityUid uid, UnionfallCapturePointComponent component, UnionfallCapturePointDoAfterEvent args)
    {
        if (args.Cancelled)
            return;
        if (args.Target is null)
            return;

        if (!TryComp<HullrotFactionComponent>(args.User, out var comp)) //someone with no faction interacted with this. modified client only
            return;
        string faction = comp.Faction;

        if (component.CapturingFaction == null) //faction now controls da point
        {
            component.CapturingFaction = faction;
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallPointCapture"), Filter.Broadcast(),
                faction + " has activated the control point! It will finish in " + float.Round(component.CurrentCaptureProgress).ToString() + " seconds.");
        }
        else if (component.CapturingFaction != faction) //opposing faction touched control point
        {
            component.CapturingFaction = faction;
            component.CurrentCaptureProgress += component.CaptureTimeBonus; //takes longer since it switched sides
            if (component.CurrentCaptureProgress > component.TimeToEnd) //cant go longer than this amount
                component.CurrentCaptureProgress = component.TimeToEnd;
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallPointCapture"), Filter.Broadcast(),
                faction + " seized control of the control point! The time left is " + float.Round(component.CurrentCaptureProgress).ToString() + " seconds.");
        }
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
                "HADAL STORM HAS DISPERSED. Emergency dispersion field has been disabled. Long-Range radar readings confirm presence of hostile fleet, with interception course set to NanoTransen Vladzena Extraction Station");
    }
}
