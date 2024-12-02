#region

using Content.Client.Botany.Components;
using Content.Shared.Botany;
using Robust.Client.GameObjects;

#endregion


namespace Content.Client.Botany;


public sealed class PotencyVisualsSystem : VisualizerSystem<PotencyVisualsComponent>
{
    protected override void OnAppearanceChange(
        EntityUid uid,
        PotencyVisualsComponent component,
        ref AppearanceChangeEvent args
    )
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<float>(uid, ProduceVisuals.Potency, out var potency, args.Component))
        {
            var scale = MathHelper.Lerp(component.MinimumScale, component.MaximumScale, potency / 100);
            args.Sprite.Scale = new(scale, scale);
        }
    }
}
