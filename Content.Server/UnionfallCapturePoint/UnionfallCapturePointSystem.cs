using Content.Shared._Crescent.ShipShields;
using Robust.Shared.Physics.Systems;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Content.Server.Power.Components;
using Content.Shared._Crescent.UnionfallCapturePoint;
using Content.Shared.Interaction;
using Content.Shared.Preferences;
using Content.Server.Preferences.Managers;
using Robust.Shared.Network;
using Content.Server._Crescent.HullrotFaction;
using Robust.Shared.Player;
using Content.Server.Announcements.Systems;
using Content.Server.GameTicking;


namespace Content.Server._Crescent.UnionfallCapturePoint;

public sealed class UnionfallCapturePointSystem : EntitySystem
{

    [Dependency] private readonly AnnouncerSystem _announcer = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<UnionfallCapturePointComponent, ActivateInWorldEvent>(OnActivatedInWorld);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<UnionfallCapturePointComponent>();
        while (query.MoveNext(out var uid, out var capturepoint))
        {

            if (capturepoint.CapturingFaction == null) //if nobody's capping it then don't do anything
                return;
            else //someone is capping it rn
            {
                capturepoint.CurrentCaptureProgress -= frameTime; //this is how the timer decreases
            }

            if (capturepoint.CurrentCaptureProgress <= 0) //capturing complete. TODO: NEED TO END THE ROUND SOMEHOW
            {
                _announcer.SendAnnouncement(_announcer.GetAnnouncementId("Fallback"), Filter.Broadcast(),
            capturepoint.CapturingFaction + " has seized control of the control point! The round is over.");
                _gameTicker.EndRound(capturepoint.CapturingFaction + " won");
                capturepoint.CurrentCaptureProgress = 999999;
            }
    }
    }

    private void OnActivatedInWorld(EntityUid uid, UnionfallCapturePointComponent component, ActivateInWorldEvent args)
    {
        if (!TryComp<HullrotFactionComponent>(args.User, out var comp)) //someone with no faction interacted with this. modified client only
            return;
        string faction = comp.Faction;

        if (component.CapturingFaction == null) //faction now controls da point
        {
            component.CapturingFaction = faction;
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("Fallback"), Filter.Broadcast(),
                faction + " is capturing the control point! Now hold it for 10 minutes!");
        }
        else if (component.CapturingFaction != faction) //opposing faction touched control point
        {
            component.CapturingFaction = faction;
            component.CurrentCaptureProgress += component.CaptureTimeBonus; //takes longer since it switched sides
            if (component.CurrentCaptureProgress > component.TimeToEnd) //cant go longer than this amount
                component.CurrentCaptureProgress = component.TimeToEnd;
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("Fallback"), Filter.Broadcast(),
                faction + " is capturing the control point!");
        }
    }
}
