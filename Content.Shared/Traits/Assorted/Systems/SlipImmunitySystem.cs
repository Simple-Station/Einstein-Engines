using Content.Shared.Slippery;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed class SlipImmunitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlipImmunityComponent, SlipAttemptEvent>(OnSlipAttempt);
    }

    private void OnSlipAttempt(EntityUid uid, SlipImmunityComponent component, ref SlipAttemptEvent args)
    {
        args.Cancel();
    }
}
