using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Map.Components;

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
         if (!TryComp<TransformComponent>(entity, out var xform))
            return;

        var parentUid = xform.ParentUid;

        // Don't delete map
        if (HasComp<MapComponent>(parentUid))
            return;

        // Prevent deleting Grids
        if (TryComp<TransformComponent>(parentUid, out var parentXform) && parentXform.GridUid == parentUid)
            return;

        EntityManager.QueueDeleteEntity(xform.ParentUid);
        args.Handled = true;
    }
}
