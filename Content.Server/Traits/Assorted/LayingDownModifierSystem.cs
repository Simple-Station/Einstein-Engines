using Content.Server.Traits.Assorted;
using Content.Shared.Standing;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed class LayingDownModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LayingDownModifierComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, LayingDownModifierComponent component, ComponentStartup args)
    {
        if (!TryComp<LayingDownComponent>(uid, out var layingDown))
            return;

        layingDown.StandingUpTime *= component.LayingDownCooldownMultiplier;
        layingDown.LyingSpeedModifier *= component.DownedSpeedMultiplierMultiplier;
    }
}
