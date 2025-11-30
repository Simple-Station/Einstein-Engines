using Content.Client._Crescent.Blocking.Components;

namespace Content.Shared._Crescent.Blocking;

public abstract class SharedBlockingSystem : EntitySystem
{
    public virtual void SetEnabled(EntityUid uid, bool value, BlockingVisualsComponent? component = null)
    {
        if (!Resolve(uid, ref component, false) || component.Enabled == value)
            return;

        component.Enabled = value;
        Dirty(uid, component);
    }
}
