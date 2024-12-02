#region

using Robust.Client.UserInterface.CustomControls;

#endregion


namespace Content.Client.Viewport;


public static class ViewportExt
{
    public static int GetRenderScale(this IViewportControl viewport)
    {
        if (viewport is ScalingViewport svp)
            return svp.CurrentRenderScale;

        return 1;
    }
}
