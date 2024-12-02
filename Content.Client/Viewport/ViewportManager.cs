#region

using Content.Client.UserInterface.Controls;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

#endregion


namespace Content.Client.Viewport;


/// <summary>
///     Event proxy for <see cref="MainViewport" /> to listen to config events.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ViewportManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private readonly List<MainViewport> _viewports = new();

    public void Initialize()
    {
        _cfg.OnValueChanged(CCVars.ViewportStretch, _ => UpdateCfg());
        _cfg.OnValueChanged(CCVars.ViewportSnapToleranceClip, _ => UpdateCfg());
        _cfg.OnValueChanged(CCVars.ViewportSnapToleranceMargin, _ => UpdateCfg());
        _cfg.OnValueChanged(CCVars.ViewportScaleRender, _ => UpdateCfg());
        _cfg.OnValueChanged(CCVars.ViewportFixedScaleFactor, _ => UpdateCfg());
    }

    private void UpdateCfg() => _viewports.ForEach(v => v.UpdateCfg());

    public void AddViewport(MainViewport vp) => _viewports.Add(vp);

    public void RemoveViewport(MainViewport vp) => _viewports.Remove(vp);
}
