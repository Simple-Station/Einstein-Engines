using Content.Shared.GameTicking;
using Content.Shared.White.Overlays;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;

namespace Content.Client.White.Overlays;

public sealed class SaturationScaleSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private SaturationScaleOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SaturationScaleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SaturationScaleComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SaturationScaleComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SaturationScaleComponent, PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeNetworkEvent<RoundRestartCleanupEvent>(RoundRestartCleanup);

        _overlay = new();
    }

    private void RoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SaturationScaleComponent component, PlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, SaturationScaleComponent component, PlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, SaturationScaleComponent component, ComponentShutdown args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }

    private void OnInit(EntityUid uid, SaturationScaleComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
            _overlayMan.AddOverlay(_overlay);
    }
}
