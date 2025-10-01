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
using Content.Shared._Crescent.UnionfallShipNode;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Destructible;


namespace Content.Server._Crescent.UnionfallCapturePoint;

public sealed class UnionfallShipNodeSystem : EntitySystem
{

    [Dependency] private readonly AnnouncerSystem _announcer = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    private ISawmill _sawmill = default!; //debug logging

    private int nodesLeftDSM = 0;
    private int nodesLeftNCWL = 0;

    private bool isRoundEnding = false; //mlg don't kill me. need this to prevent round bugging out when the components get deleted.

    public override void Initialize()
    {
        SubscribeLocalEvent<UnionfallShipNodeComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<UnionfallShipNodeComponent, ActivateInWorldEvent>(OnActivatedInWorld);
        SubscribeLocalEvent<UnionfallShipNodeComponent, UnionfallShipNodeDoAfterEvent>(OnCaptureDoAfter);
        SubscribeLocalEvent<UnionfallShipNodeComponent, DestructionEventArgs>(OnDestruction);
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("unionfall.shipnodes");
    }

    private void OnComponentInit(EntityUid uid, UnionfallShipNodeComponent component, ComponentInit args)
    {
        isRoundEnding = false;
        if (component.OwningFaction == "DSM")
            nodesLeftDSM += 1;
        else if (component.OwningFaction == "NCWL") //could be "else" but just in case we get more factions
            nodesLeftNCWL += 1;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<UnionfallShipNodeComponent>();
        while (query.MoveNext(out var uid, out var capturepoint))
        {
            capturepoint.GracePeriod -= frameTime; //we do it this way so we can VVedit in admin mode midgame
            if (capturepoint.GracePeriod > 0f) //point is still in grace period
                continue;
            if (capturepoint.IsBeingCaptured == false) //if nobody's capping it then don't do anything
                continue;
            else //someone is capping it rn
            {
                capturepoint.CurrentCaptureProgress -= frameTime; //this is how the timer decreases
            }

            if (capturepoint.CurrentCaptureProgress <= 0) //capturing complete. announce and count how many left
            {
                var eventArgs = new DestructionEventArgs();

                RaiseLocalEvent(uid, eventArgs); //should force OnDestruction to fire and do the same thing as when ti gets destroyed
                QueueDel(uid);

            }
        }
    }

    private void OnActivatedInWorld(EntityUid uid, UnionfallShipNodeComponent component, ActivateInWorldEvent args)
    {
        if (component.GracePeriod > 0) //grace period still active
        {
            _popup.PopupEntity(Loc.GetString("shipnode-grace-period-fail"), uid, args.User);
            return;
        }

        if (!TryComp<HullrotFactionComponent>(args.User, out var comp)) //someone with no faction interacted with this. modified client only
            return;
        string faction = comp.Faction;

        if (component.OwningFaction == faction & component.IsBeingCaptured == false)
        {
            _popup.PopupEntity(Loc.GetString("shipnode-same-faction-fail"), uid, args.User);
            return;
        }

        if (component.OwningFaction == faction) //defusing
            _popup.PopupEntity(Loc.GetString("shipnode-defusing"), uid, args.User);
        else
            _popup.PopupEntity(Loc.GetString("shipnode-sabotaging"), uid, args.User);


        DoAfterArgs doAfterArguments = new DoAfterArgs(EntityManager, args.User, component.DoAfterDelay, new UnionfallShipNodeDoAfterEvent(), uid, uid, null)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArguments, null);
    }

    private void OnCaptureDoAfter(EntityUid uid, UnionfallShipNodeComponent component, UnionfallShipNodeDoAfterEvent args)
    {
        if (args.Cancelled)
            return;
        if (args.Target is null)
            return;

        if (!TryComp<HullrotFactionComponent>(args.User, out var comp)) //someone with no faction interacted with this. modified client only
            return;
        string faction = comp.Faction;

        if (component.OwningFaction != comp.Faction) // opposing faction rigged to blow
        {
            component.IsBeingCaptured = true;
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallPointCapture"), Filter.Broadcast(),
                "A " + component.OwningFaction + " cloner database has been rigged to explode! It will detonate in " + float.Round(component.CurrentCaptureProgress).ToString() + " seconds.");
        }
        else if (component.OwningFaction == faction) // same faction interacted to defuse
        {
            component.IsBeingCaptured = false;
            component.CurrentCaptureProgress = component.TimeToCapture;

            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("unionfallPointCapture"), Filter.Broadcast(),
                "The " + component.OwningFaction + " cloner database has been defused.");
        }
    }

    private void OnDestruction(EntityUid uid, UnionfallShipNodeComponent capturepoint, DestructionEventArgs args)
    {
        _explosionSystem.TriggerExplosive(uid);
        if (isRoundEnding) //prevents this from triggering 6 times and breaking the round when they all get removed from the game
            return;
        if (capturepoint.OwningFaction == "NCWL")
            nodesLeftNCWL -= 1;
        else if (capturepoint.OwningFaction == "DSM")
            nodesLeftDSM -= 1;

        if (nodesLeftNCWL <= 0 || nodesLeftDSM <= 0)
        {
            isRoundEnding = true;
            nodesLeftDSM = 0; //prep for next round
            nodesLeftNCWL = 0;
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("Fallback"), Filter.Broadcast(),
            capturepoint.OwningFaction + " has lost all of their warships and cloner databases. They are doomed to a slow death in Taypan.");
            _gameTicker.EndRound("All of " + capturepoint.OwningFaction + "'s cloner databases have been destroyed. ROUND OVER");
            capturepoint.CurrentCaptureProgress = 999999;
            Timer.Spawn(TimeSpan.FromMinutes(1), _gameTicker.RestartRound);
        }
        else 
        {
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId("Fallback"), Filter.Broadcast(),
            "A " + capturepoint.OwningFaction + " cloner database has been destroyed! | REMAINING FOR DSM: " + nodesLeftDSM + " | REMAINING FOR NCWL: " + nodesLeftNCWL);
        }
    }


}
