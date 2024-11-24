using Content.Shared.DragDrop;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Content.Shared.Nyanotrasen.Kitchen.Components;
using Content.Shared.Item;
using Content.Shared.Body.Components;

namespace Content.Shared.Nyanotrasen.Kitchen;

public abstract class SharedDeepfryerSystem : EntitySystem
{
    protected void OnCanDragDropOn(EntityUid uid, SharedDeepFryerComponent component, ref CanDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.CanDrop = CanInsert(uid, args.Dragged);
        args.Handled = true;
    }

    public virtual bool CanInsert(EntityUid uid, EntityUid entity)
    {
        if (!Transform(uid).Anchored)
            return false;

        var storable = HasComp<ItemComponent>(entity);
        if (!storable && !HasComp<BodyComponent>(entity))
            return false;

        if (TryComp<PhysicsComponent>(entity, out var physics) && (physics.CanCollide) || storable)
            return true;
        else
            return false;

    }
}
