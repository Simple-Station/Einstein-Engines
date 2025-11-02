using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Crescent.HullrotSelfDeleteTimer;

public sealed class HullrotSelfDeleteSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntityManager _IentityManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _station = default!;

    private float _distanceToDeleteItems = 128;
    private TimeSpan _delayBetweenItemDeleteAttempts = TimeSpan.FromMinutes(1); //more aggressive to hopefully help with lag
    private float _distanceToDeleteGrids = 256;
    private TimeSpan _delayBetweenGridDeleteAttempts = TimeSpan.FromMinutes(5);

    //base time in SelfDeleteInSpaceComponent on init: TimeSpan.FromSeconds(60);
    //base time in SelfDeleteGrid on init: TimeSpan.FromMinutes(20);


    public override void Initialize()
    {
        SubscribeLocalEvent<SelfDeleteComponent, ComponentInit>(OnInitEntity);
        SubscribeLocalEvent<SelfDeleteGridComponent, ComponentInit>(OnInitGrid);
        SubscribeLocalEvent<SelfDeleteInSpaceComponent, EntParentChangedMessage>(OnParentChanged);
    }

    private void OnInitEntity(EntityUid uid, SelfDeleteComponent component, ComponentInit args)
    {
        Timer.Spawn(component.TimeToDelete, () => DeleteEntity(uid));
    }

    private void OnInitGrid(EntityUid uid, SelfDeleteGridComponent component, ComponentInit args)
    {
        Timer.Spawn(component.TimeToDelete, () => DeleteGrid(uid));
    }

    private void OnParentChanged(EntityUid uid, SelfDeleteInSpaceComponent component, EntParentChangedMessage args)
    {
        if (uid == EntityUid.Invalid)
            return;
        if (_station.GetOwningStation(uid) == null) //this returns null when we are in space
            Timer.Spawn(component.TimeToDelete, () => TryDeleteEntityInSpace(uid));
    }

    private void DeleteEntity(EntityUid uid)
    {
        _IentityManager.DeleteEntity(uid);
    }

    // in hullrot, entities with the selfdeletinginspace component call this after one minute.
    // first, we check if the owning station is null (empty space). if so, THEN we run through a check for all the current
    // actors (players) being nearby. if yes, don't delete the item and try again in a whopping 5 minutes where
    // hopefully they fucked off somewhere else. MOST items will get cleaned up in 1 minute after being tossed into space.
    private void TryDeleteEntityInSpace(EntityUid uid)
    {
        if (!TryComp<TransformComponent>(uid, out var _)) //was throwing errors because somehow entities with no transform component were getting this called
            return;

        if (_station.GetOwningStation(uid) == null)
        {
            var enumerator = EntityManager.EntityQueryEnumerator<ActorComponent>(); //only players are actors. i think.

            while (enumerator.MoveNext(out var actorUid, out var _))
            {
                if ((_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(actorUid)).Length() < _distanceToDeleteItems)
                {
                    Timer.Spawn(_delayBetweenItemDeleteAttempts, () => TryDeleteEntityInSpace(uid));
                    return;
                }
            }
            _IentityManager.DeleteEntity(uid);
        }
        else
            Timer.Spawn(_delayBetweenItemDeleteAttempts, () => TryDeleteEntityInSpace(uid));
    }

    // in hullrot, this is used for self-deleting grids that blast off of ships after a while.
    // we run an additional check when it's delete time to see if anybody is within 256 tiles of the deleting grid
    // so that there's no chance of accidentally deleting a player, and of grids vanishing before your eyes.
    // if that check fails, fire another timer and try again.
    private void DeleteGrid(EntityUid uid)
    {
        var enumerator = EntityManager.EntityQueryEnumerator<ActorComponent>(); //only players are actors. i think.

        while (enumerator.MoveNext(out var actorUid, out var _))
        {
            if ((_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(actorUid)).Length() < _distanceToDeleteGrids)
            {
                Timer.Spawn(_delayBetweenGridDeleteAttempts, () => DeleteGrid(uid));
                return;
            }
        }
        _IentityManager.DeleteEntity(uid);
    }



}
