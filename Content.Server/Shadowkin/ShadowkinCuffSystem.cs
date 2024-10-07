using Content.Shared.Inventory.Events;
using Content.Shared.Clothing.Components;
using Content.Shared.Shadowkin;

namespace Content.Server.Shadowkin;

public sealed class ShadowkinCuffSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowkinCuffComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ShadowkinCuffComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, ShadowkinCuffComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing)
            || !clothing.Slots.HasFlag(args.SlotFlags))
            return;

        EnsureComp<ShadowkinCuffComponent>(args.Equipee);
    }

    private void OnUnequipped(EntityUid uid, ShadowkinCuffComponent component, GotUnequippedEvent args)
    {
        RemComp<ShadowkinCuffComponent>(args.Equipee);
    }
}