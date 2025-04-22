using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Shared._EE.Shadowling.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._EE.Shadowling;


/// <summary>
/// This handles status icons for slings and thralls
/// </summary>
public sealed class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, GetStatusIconsEvent>(GetThrallIcon);
        SubscribeLocalEvent<LightDetectionDamageModifierComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(GetShadowlingIcon);
    }

    private void OnUpdateAlert(EntityUid uid, LightDetectionDamageModifierComponent comp, UpdateAlertSpriteEvent args)
    {
        var normalized = (int)(comp.DetectionValue / comp.DetectionValueMax * comp.AlertSprites);

        var sprite = args.SpriteViewEnt.Comp;
        sprite.LayerSetState(AlertVisualLayers.Base, $"{normalized}");
    }
    private void GetThrallIcon(Entity<ThrallComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<ShadowlingComponent>(ent))
            return;

        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void GetShadowlingIcon(Entity<ShadowlingComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
