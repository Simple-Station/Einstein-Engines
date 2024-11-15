using Robust.Client.GameObjects;
using static Robust.Client.GameObjects.SpriteComponent;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Paint;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.Paint;

public sealed class PaintedVisualizerSystem : VisualizerSystem<PaintedComponent>
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaintedComponent, HeldVisualsUpdatedEvent>(OnHeldVisualsUpdated);
        SubscribeLocalEvent<PaintedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PaintedComponent, EquipmentVisualsUpdatedEvent>(OnEquipmentVisualsUpdated);
    }


    protected override void OnAppearanceChange(EntityUid uid, PaintedComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !_appearance.TryGetData(uid, PaintVisuals.Painted, out bool isPainted))
            return;

        var shader = _protoMan.Index<ShaderPrototype>(component.ShaderName).Instance();
        foreach (var spriteLayer in args.Sprite.AllLayers)
        {
            if (spriteLayer is not Layer layer)
                continue;

            if (layer.Shader == null || layer.Shader == shader)
            {
                layer.Shader = shader;
                layer.Color = component.Color;
            }
        }
    }

    private void OnShutdown(EntityUid uid, PaintedComponent component, ref ComponentShutdown args)
    {
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;
        component.BeforeColor = sprite.Color;

        if (Terminating(uid))
            return;

        foreach (var spriteLayer in sprite.AllLayers)
        {
            if (spriteLayer is not Layer layer
                || layer.Shader != _protoMan.Index<ShaderPrototype>(component.ShaderName).Instance())
                continue;

            layer.Shader = null;
            if (layer.Color == component.Color)
                layer.Color = component.BeforeColor;
        }
    }

    private void OnHeldVisualsUpdated(EntityUid uid, PaintedComponent component, HeldVisualsUpdatedEvent args) =>
        UpdateVisuals(component, args);
    private void OnEquipmentVisualsUpdated(EntityUid uid, PaintedComponent component, EquipmentVisualsUpdatedEvent args) =>
        UpdateVisuals(component, args);
    private void UpdateVisuals(PaintedComponent component, EntityEventArgs args)
    {
        var layers = new HashSet<string>();
        var entity = EntityUid.Invalid;

        switch (args)
        {
            case HeldVisualsUpdatedEvent hgs:
                layers = hgs.RevealedLayers;
                entity = hgs.User;
                break;
            case EquipmentVisualsUpdatedEvent eqs:
                layers = eqs.RevealedLayers;
                entity = eqs.Equipee;
                break;
        }

        if (layers.Count == 0 || !TryComp(entity, out SpriteComponent? sprite))
            return;

        foreach (var revealed in layers)
        {
            if (!sprite.LayerMapTryGet(revealed, out var layer))
                continue;

            sprite.LayerSetShader(layer, component.ShaderName);
            sprite.LayerSetColor(layer, component.Color);
        }
    }
}
