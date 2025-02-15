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

        if (HasComp<MapComponent>(xform.ParentUid)) // don't delete map
            return;

        EntityManager.QueueDeleteEntity(xform.ParentUid);
        args.Handled = true;
    }
}
