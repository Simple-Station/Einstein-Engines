using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;


namespace Content.Server._Goobstation.Explosion.EntitySystems;


public sealed partial class GoobTriggerSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
    }

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {

        var uid = entity.Owner;
        var userXform = Transform(uid);

        if (userXform.ParentUid == userXform.GridUid || userXform.ParentUid == userXform.MapUid)
            return;

        EntityManager.QueueDeleteEntity(userXform.ParentUid);
        args.Handled = true;
    }

}
