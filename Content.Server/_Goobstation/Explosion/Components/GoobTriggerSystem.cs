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

        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
    }

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {
        var uid = entity.Owner;

        if (!TryComp<TransformComponent>(uid, out var xform))
            return;

        var parentUid = xform.ParentUid;

        // Check if the parent entity has any component that implements INonDeletable
        if (EntityManager.GetComponents(parentUid).Any(c => c is INonDeletable))
            return;

        EntityManager.QueueDeleteEntity(parentUid);
        args.Handled = true;
    }


}
