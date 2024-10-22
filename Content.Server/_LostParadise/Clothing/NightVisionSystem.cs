using Content.Shared._LostParadise.Clothing;
using Content.Shared.Alert;
using Content.Shared.Inventory.Events;

namespace Content.Server._LostParadise.Clothing;

/// <summary>
/// Made by BL02DL from _LostParadise
/// </summary>

public sealed class NightVisionSystem : SharedNightVisionSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<NightVisionComponent, GotUnequippedEvent>(OnGotUnequipped);

    }

    protected override void UpdateNightVisionEffects(EntityUid parent, EntityUid uid, bool state, NightVisionComponent? component)
    {
        if (!Resolve(uid, ref component))
            return;
        state = state && component.On;

        if (state)
        {
            _alertsSystem.ShowAlert(parent, AlertType.LPPNightVision);
        }
        else
        {
            _alertsSystem.ClearAlert(parent, AlertType.LPPNightVision);
        }
    }

    private void OnGotUnequipped(EntityUid uid, NightVisionComponent component, GotUnequippedEvent args)
    {
        if (args.Slot == "eyes")
        {
            UpdateNightVisionEffects(args.Equipee, uid, false, component);
            _alertsSystem.ClearAlert(uid, AlertType.LPPNightVision);
        }
    }

    private void OnGotEquipped(EntityUid uid, NightVisionComponent component, GotEquippedEvent args)
    {
        if (args.Slot == "eyes")
        {
            UpdateNightVisionEffects(args.Equipee, uid, true, component);
        }
    }
}
