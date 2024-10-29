/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

using Robust.Client.GameObjects;
using System.Numerics;
using Content.Shared.SegmentedEntity;

namespace Content.Client.DeltaV.Lamiae;

public sealed class ClientLamiaVisualSystem : VisualizerSystem<SegmentedEntitySegmentVisualsComponent>
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SegmentedEntitySegmentComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }
    private void OnAppearanceChange(EntityUid uid, SegmentedEntitySegmentComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null) return;

        if (AppearanceSystem.TryGetData<float>(uid, ScaleVisuals.Scale, out var scale) && TryComp<SpriteComponent>(uid, out var sprite))
        {
            sprite.Scale = new Vector2(scale, scale);
        }

        if (AppearanceSystem.TryGetData<bool>(uid, SegmentedEntitySegmentVisualLayers.Armor, out var worn)
            && AppearanceSystem.TryGetData<string>(uid, SegmentedEntitySegmentVisualLayers.ArmorRsi, out var path))
        {
            var valid = !string.IsNullOrWhiteSpace(path);
            if (valid)
            {
                args.Sprite.LayerSetRSI(SegmentedEntitySegmentVisualLayers.Armor, path);
            }
            args.Sprite.LayerSetVisible(SegmentedEntitySegmentVisualLayers.Armor, worn);
        }
    }
}
