#region

using Content.Shared.CCVar;
using Content.Shared.OfferItem;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Configuration;

#endregion


namespace Content.Client.OfferItem;


public sealed class OfferItemSystem : SharedOfferItemSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEyeManager _eye = default!;

    public override void Initialize() =>
        Subs.CVar(_cfg, CCVars.OfferModeIndicatorsPointShow, OnShowOfferIndicatorsChanged, true);

    public override void Shutdown()
    {
        _overlayManager.RemoveOverlay<OfferItemIndicatorsOverlay>();

        base.Shutdown();
    }

    public bool IsInOfferMode()
    {
        var entity = _playerManager.LocalEntity;

        if (entity == null)
            return false;

        return IsInOfferMode(entity.Value);
    }

    private void OnShowOfferIndicatorsChanged(bool isShow)
    {
        if (isShow)
        {
            _overlayManager.AddOverlay(
                new OfferItemIndicatorsOverlay(
                    _inputManager,
                    EntityManager,
                    _eye,
                    this));
        }
        else
            _overlayManager.RemoveOverlay<OfferItemIndicatorsOverlay>();
    }
}
