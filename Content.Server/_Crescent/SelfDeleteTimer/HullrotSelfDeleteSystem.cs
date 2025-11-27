using Content.Server.EventScheduler;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Crescent.HullrotSelfDeleteTimer;

public sealed class HullrotSelfDeleteSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntityManager _IentityManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IMapManager _mappingManager = default!;
    [Dependency] private readonly EventSchedulerSystem _eventScheduler = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private TimeSpan _roundstartDelayBeforeSystemActivates = TimeSpan.FromSeconds(30); //needed because some shit like salvage is initially parented to space...for some reason
    private float _distanceToDeleteItems = 64;
    private TimeSpan _delayBetweenItemDeleteAttempts = TimeSpan.FromMinutes(2); //more aggressive to hopefully help with lag
    private float _distanceToDeleteGrids = 256;
    private TimeSpan _delayBetweenGridDeleteAttempts = TimeSpan.FromMinutes(10);

    private ISawmill _sawmill = default!; //logging

    //base time in SelfDeleteInSpaceComponent on init: TimeSpan.FromSeconds(60);
    //base time in SelfDeleteGrid on init: TimeSpan.FromMinutes(20);


    public override void Initialize()
    {
        SubscribeLocalEvent<SelfDeleteComponent, ComponentInit>(OnInitEntity); //used for autodeleting stuff manually

        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("hullrot.cleanup");

        //_sawmill.Debug("it's showtime");
        SubscribeLocalEvent<SelfDeleteGridComponent, ComponentInit>(OnInitGrid); //used for cleaning up debris grids
        SubscribeLocalEvent<SelfDeleteInSpaceComponent, EntParentChangedMessage>(OnParentChanged); //used for cleaning up items in space

        SubscribeLocalEvent<SelfDeleteGridComponent, HullrotAttemptCleanupGrid>(DeleteGrid);
        SubscribeLocalEvent<SelfDeleteInSpaceComponent, HullrotAttemptCleanupItem>(TryDeleteEntityInSpace);
    }

    private void OnInitEntity(EntityUid uid, SelfDeleteComponent component, ComponentInit args)
    {
        Timer.Spawn(component.TimeToDelete, () => DeleteEntity(uid));
    }

    private void OnInitGrid(EntityUid uid, SelfDeleteGridComponent component, ComponentInit args)
    {
        // .2 note: i have **no idea** why this doesn't fucking work here
        // if (Name(uid) != "grid") //we ONLY want this to spawn on debris grids that break off, not on anything else
        //     return;              //as you might have guessed, these are always called "grid"
        // RemComp<SelfDeleteGridComponent>(uid); // we have to add it to all grids on gridinit in shuttlesystem, so we delete it here if it's not meant to have it.
        var dEv = new HullrotAttemptCleanupGrid();
        _eventScheduler.DelayEvent(uid, ref dEv, _delayBetweenGridDeleteAttempts);
    }

    private void OnParentChanged(EntityUid uid, SelfDeleteInSpaceComponent component, EntParentChangedMessage args)
    {
        if (uid == EntityUid.Invalid)
            return;
        if (_gameTicker.RoundDuration() < _roundstartDelayBeforeSystemActivates) //salvage items spawn in space and are not parented to their grids for some reason
            return;         // this lets them spawn in and not get autodeleted instantly
        if (args.Transform.GridUid == null) //this returns null when we are in space
        {
            //_sawmill.Debug("item went into space: " + Name(uid) + " - deleting in " + _delayBetweenItemDeleteAttempts.ToString());
            var dEv = new HullrotAttemptCleanupItem();
            _eventScheduler.DelayEvent(uid, ref dEv, _delayBetweenItemDeleteAttempts);
        }
    }

    private void DeleteEntity(EntityUid uid)
    {
        _IentityManager.DeleteEntity(uid);
    }

    // in hullrot, entities with the selfdeletinginspace component call this after one minute.
    // first, we check if the owning station is null (empty space). if so, THEN we run through a check for all the current
    // actors (players) being nearby. if yes, don't delete the item and try again in a whopping 5 minutes where
    // hopefully they fucked off somewhere else. MOST items will get cleaned up in 1 minute after being tossed into space.
    private void TryDeleteEntityInSpace(EntityUid uid, SelfDeleteInSpaceComponent component, HullrotAttemptCleanupItem args)
    {
        if (!TryComp<MetaDataComponent>(uid, out var _)) //was throwing errors because somehow entities with no metadata component were getting this called
            return;

        if (!TryComp<TransformComponent>(uid, out var transformComp)) //was throwing errors because somehow entities with no transform component were getting this called
            return;

        if (!_mappingManager.IsMapInitialized(transformComp.MapID)) //if the map is NOT initialized, then WE ARE IN MAPPING MODE!!! SO DON'T DO SHIT!!!!
            return;

        //_sawmill.Debug("1. seeing if ent " + Name(uid) + "is in space...");
        if (transformComp.GridUid == null) //are we STILL in space?
        {
            //_sawmill.Debug("2. trying to delete entity + " + Name(uid) + "...");
            var enumerator = EntityManager.EntityQueryEnumerator<ActorComponent>(); //should only detect players, but this fails for some reason.
            while (enumerator.MoveNext(out var actorUid, out var _))
            {
                //_sawmill.Debug("3. checking distance between entity (" + Name(uid) + "), [" + _transform.GetWorldPosition(uid).ToString() + "], and actor " + Name(actorUid) + ", [" + _transform.GetWorldPosition(actorUid).ToString() + "], length = " + (_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(actorUid)).Length().ToString());
                if ((_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(actorUid)).Length() < _distanceToDeleteItems)
                {
                    var dEv = new HullrotAttemptCleanupItem();
                    _eventScheduler.DelayEvent(uid, ref dEv, _delayBetweenItemDeleteAttempts);
                    return;
                }
            }
            _IentityManager.DeleteEntity(uid);
        }
        else //we are NOT in space anymore. don't try again
        {
            return;
        }
    }

    // in hullrot, this is used for self-deleting grids that blast off of ships after a while.
    // we run an additional check when it's delete time to see if anybody is within 256 tiles of the deleting grid
    // so that there's no chance of accidentally deleting a player, and of grids vanishing before your eyes.
    // if that check fails, fire another timer and try again.
    private void DeleteGrid(EntityUid uid, SelfDeleteGridComponent component, HullrotAttemptCleanupGrid args)
    {
        // I KNOW THIS WORKS
        if (Name(uid) != "grid") //we ONLY want this to spawn on debris grids that break off, not on anything else
        {
            RemComp<SelfDeleteGridComponent>(uid);//as you might have guessed, these are always called "grid" 
            return;
        }

        if (!TryComp<MetaDataComponent>(uid, out var _)) //was throwing errors because somehow entities with no metadata component were getting this called
            return;

        if (!TryComp<TransformComponent>(uid, out var transformComp)) //was throwing errors because somehow entities with no transform component were getting this called
            return;

        if (!_mappingManager.IsMapInitialized(transformComp.MapID)) //if the map is NOT initialized, then WE ARE IN MAPPING MODE!!! SO DON'T DO SHIT!!!!
            return;

        var enumerator = EntityManager.EntityQueryEnumerator<ActorComponent>(); //only players are actors. i think.
        while (enumerator.MoveNext(out var actorUid, out var _))
        {
            if ((_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(actorUid)).Length() < _distanceToDeleteGrids)
            {
                var dEv = new HullrotAttemptCleanupGrid();
                _eventScheduler.DelayEvent(uid, ref dEv, _delayBetweenGridDeleteAttempts);
                return;
            }
        }
        _IentityManager.DeleteEntity(uid);
    }



}

[ByRefEvent]
public record struct HullrotAttemptCleanupGrid();

[ByRefEvent]
public record struct HullrotAttemptCleanupItem();
