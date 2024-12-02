#region

using Robust.Client.GameObjects;
using static Content.Shared.Paper.SharedPaperComponent;

#endregion


namespace Content.Client.Paper;


public sealed class PaperSystem : VisualizerSystem<PaperVisualsComponent>
{
    protected override void OnAppearanceChange(
        EntityUid uid,
        PaperVisualsComponent component,
        ref AppearanceChangeEvent args
    )
    {
        if (args.Sprite == null)
            return;

        if (Robust.Client.GameObjects.AppearanceSystem.TryGetData<PaperStatus>(
            uid,
            PaperVisuals.Status,
            out var writingStatus,
            args.Component))
            args.Sprite.LayerSetVisible(PaperVisualLayers.Writing, writingStatus == PaperStatus.Written);

        if (Robust.Client.GameObjects.AppearanceSystem.TryGetData<string>(
            uid,
            PaperVisuals.Stamp,
            out var stampState,
            args.Component))
        {
            args.Sprite.LayerSetState(PaperVisualLayers.Stamp, stampState);
            args.Sprite.LayerSetVisible(PaperVisualLayers.Stamp, true);
        }
    }
}

public enum PaperVisualLayers
{
    Stamp,
    Writing
}
