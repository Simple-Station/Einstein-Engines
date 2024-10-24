using Content.Shared.Traits.Assorted.Components;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client.Overlays;

public sealed partial class UltraVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private UltraVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UltraVisionComponent, ComponentInit>(OnUltraVisionInit);
        SubscribeLocalEvent<UltraVisionComponent, ComponentShutdown>(OnUltraVisionShutdown);
        SubscribeLocalEvent<UltraVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<UltraVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, CCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnUltraVisionInit(EntityUid uid, UltraVisionComponent component, ComponentInit args)
    {
        if (uid != _playerMan.LocalEntity)
            return;

        if (!_cfg.GetCVar(CCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnUltraVisionShutdown(EntityUid uid, UltraVisionComponent component, ComponentShutdown args)
    {
        if (uid != _playerMan.LocalEntity)
            return;

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, UltraVisionComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(CCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, UltraVisionComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
