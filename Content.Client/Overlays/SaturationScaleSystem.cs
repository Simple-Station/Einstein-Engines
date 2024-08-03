using Content.Shared.GameTicking;
using Content.Shared.Overlays;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Overlays;

public sealed class SaturationScaleSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

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
        if (_player.LocalSession?.AttachedEntity != uid)
            return;

        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnInit(EntityUid uid, SaturationScaleComponent component, ComponentInit args)
    {
        if (_player.LocalSession?.AttachedEntity != uid)
            return;

        _overlayMan.AddOverlay(_overlay);
    }
}
