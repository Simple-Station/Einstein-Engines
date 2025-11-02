using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Crescent.SelfDeleteTimer;

public sealed class SelfDeleteSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntityManager _IentityManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<SelfDeleteComponent, ComponentInit>(OnInitEntity);
        SubscribeLocalEvent<SelfDeleteGridComponent, ComponentInit>(OnInitGrid);
    }

    private void OnInitEntity(EntityUid uid, SelfDeleteComponent component, ComponentInit args)
    {
        Timer.Spawn(component.TimeToDelete, () => DeleteEntity(uid));
    }

    private void OnInitGrid(EntityUid uid, SelfDeleteGridComponent component, ComponentInit args)
    {
        Timer.Spawn(component.TimeToDelete, () => DeleteGrid(uid));
    }

    private void DeleteEntity(EntityUid uid)
    {
        //_entityManager.QueueDeleteEntity(uid); this worked before but Ientity might work
        _IentityManager.DeleteEntity(uid);
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
            if ((_transform.GetWorldPosition(uid) - _transform.GetWorldPosition(actorUid)).Length() < 256f)
            {
                Timer.Spawn(TimeSpan.FromMinutes(5), () => DeleteGrid(uid));
                return;
            }
        }
        _IentityManager.DeleteEntity(uid);
    }

}
