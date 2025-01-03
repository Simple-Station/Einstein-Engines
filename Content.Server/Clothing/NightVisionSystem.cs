using Content.Shared.Clothing;
using Content.Shared.Inventory.Events;

namespace Content.Server.Clothing;

/// <summary>
///     Made by BL02DL from _LostParadise
/// </summary>

public sealed class NightVisionSystem : SharedNightVisionSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<NightVisionComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotUnequipped(EntityUid uid, NightVisionComponent component, GotUnequippedEvent args)
    {
        if (args.Slot == component.Slot)
            UpdateNightVisionEffects(args.Equipee, uid, false, component);
    }

    private void OnGotEquipped(EntityUid uid, NightVisionComponent component, GotEquippedEvent args)
    {
        if (args.Slot == component.Slot)
            UpdateNightVisionEffects(args.Equipee, uid, true, component);
    }
}
