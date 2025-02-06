using System.Reflection.Metadata;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays.Switchable;
using Robust.Client.Graphics;

namespace Content.Client.Overlays.Switchable;

public sealed class NightVisionSystem : EquipmentHudSystem<NightVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private BaseSwitchableOverlay<NightVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _overlay = new BaseSwitchableOverlay<NightVisionComponent>();
    }

    protected override void OnRefreshComponentHud(EntityUid uid,
        NightVisionComponent component,
        RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        if (component.IsEquipment)
            return;

        base.OnRefreshComponentHud(uid, component, args);
    }

    protected override void OnRefreshEquipmentHud(EntityUid uid,
        NightVisionComponent component,
        InventoryRelayedEvent<RefreshEquipmentHudEvent<NightVisionComponent>> args)
    {
        if (!component.IsEquipment)
            return;

        base.OnRefreshEquipmentHud(uid, component, args);
    }

    private void OnToggle(Entity<NightVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay(args.User);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        base.UpdateInternal(args);

        var active = false;
        NightVisionComponent? nvComp = null;
        foreach (var comp in args.Components)
        {
            if (!comp.IsActive && (comp.PulseTime <= TimeSpan.Zero || comp.PulseAccumulator >= comp.PulseTime))
                continue;

            if (nvComp == null)
                nvComp = comp;
            else if (!nvComp.DrawOverlay && comp.DrawOverlay)
                nvComp = comp;
            else if (nvComp.DrawOverlay == comp.DrawOverlay && nvComp.PulseTime > TimeSpan.Zero && comp.PulseTime <= TimeSpan.Zero)
                nvComp = comp;
        }

        UpdateNightVision(active);
        UpdateOverlay(nvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        UpdateNightVision(false);
        UpdateOverlay(null);
    }

    private void UpdateNightVision(bool active)
    {
        _lightManager.DrawLighting = !active;
    }

    private void UpdateOverlay(NightVisionComponent? nvComp)
    {
        _overlay.Comp = nvComp;

        switch (nvComp)
        {
            case not null when !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        if (_overlayMan.TryGetOverlay<BaseSwitchableOverlay<ThermalVisionComponent>>(out var overlay))
            overlay.IsActive = nvComp == null;
    }
}
