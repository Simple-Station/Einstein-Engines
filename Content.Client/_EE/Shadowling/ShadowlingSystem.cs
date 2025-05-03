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
/// This also handles alerts
/// </summary>
public sealed class ShadowlingSystem : SharedShadowlingSystem
{
    private const int StateNormalizerSling = 9;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, GetStatusIconsEvent>(GetThrallIcon);
        SubscribeLocalEvent<ShadowlingComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
        SubscribeLocalEvent<LesserShadowlingComponent, UpdateAlertSpriteEvent>(OnUpdateAlertLesser);
        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(GetShadowlingIcon);
    }

    private void OnUpdateAlert(Entity<ShadowlingComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.AlertProto)
            return;

        if (!EntityManager.TryGetComponent<LightDetectionDamageModifierComponent>(
            ent,
            out var lightDetectionDamageModifier))
            return;

        var sprite = args.SpriteViewEnt.Comp;
        var normalized = (int)( (lightDetectionDamageModifier.DetectionValue / lightDetectionDamageModifier.DetectionValueMax) * StateNormalizerSling );
        normalized = Math.Clamp(normalized, 0, StateNormalizerSling);

        sprite.LayerSetState(AlertVisualLayers.Base, $"{normalized}");
    }

    private void OnUpdateAlertLesser(Entity<LesserShadowlingComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.AlertProto)
            return;

        if (!EntityManager.TryGetComponent<LightDetectionDamageModifierComponent>(
            ent,
            out var lightDetectionDamageModifier))
            return;

        var sprite = args.SpriteViewEnt.Comp;
        var normalized = (int)( (lightDetectionDamageModifier.DetectionValue / lightDetectionDamageModifier.DetectionValueMax) * StateNormalizerSling );
        normalized = Math.Clamp(normalized, 0, StateNormalizerSling);

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
