using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using System.Linq;

namespace Content.Server._Goobstation.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem : EntitySystem
{
    public interface INonDeletable;
    public override void Initialize()
    {
        base.Initialize();

@@ -14,10 +16,20 @@ public override void Initialize()

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {
        if (!TryComp<TransformComponent>(entity, out var xform))
        var uid = entity.Owner;

        if (!TryComp<TransformComponent>(uid, out var xform))
            return;

        var parentUid = xform.ParentUid;

        // Check if the parent entity has any component that implements INonDeletable
        if (EntityManager.GetComponents(parentUid).Any(c => c is INonDeletable))
            return;

        EntityManager.QueueDeleteEntity(xform.ParentUid);
        EntityManager.QueueDeleteEntity(parentUid);
        args.Handled = true;
    }

}
