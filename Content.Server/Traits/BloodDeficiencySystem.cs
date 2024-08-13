using Content.Server.Body.Systems;
using Content.Server.Body.Components;
using Content.Shared.Damage;

namespace Content.Server.Traits.Assorted;

public sealed class BloodDeficiencySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodDeficiencyComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, BloodDeficiencyComponent component, ComponentStartup args)
    {
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        bloodstream.HasBloodDeficiency = true;
        bloodstream.BloodDeficiencyLossAmount = component.BloodLossAmount;
    }
}
