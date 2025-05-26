using Content.Shared.Body.Events;
using Content.Server.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Body.Organ;
using Content.Server._Shitmed.DelayedDeath;

namespace Content.Server._Shitmed.Body.Organ;

public sealed class CerebralImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CerebralImplantComponent, OrganAddedToBodyEvent>(HandleAddition);
        SubscribeLocalEvent<CerebralImplantComponent, OrganRemovedFromBodyEvent>(HandleRemoval);
    }

    private void HandleRemoval(EntityUid uid, CerebralImplantComponent _, ref OrganRemovedFromBodyEvent args)
    {
        // Placeholder: No action taken when removed.
    }

    private void HandleAddition(EntityUid uid, CerebralImplantComponent _, ref OrganAddedToBodyEvent args)
    {
        // Placeholder: No action taken when added.
    }
}
