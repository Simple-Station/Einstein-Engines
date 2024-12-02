#region

using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Utility;

#endregion


namespace Content.Client.OfferItem;


public sealed class OfferItemIndicatorsOverlay : Overlay
{
    private readonly IInputManager _inputManager;
    private readonly IEntityManager _entMan;
    private readonly IEyeManager _eye;
    private readonly OfferItemSystem _offer;

    private readonly Texture _sight;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    private readonly Color _mainColor = Color.White.WithAlpha(0.3f);
    private readonly Color _strokeColor = Color.Black.WithAlpha(0.5f);
    private readonly float _scale = 0.6f; // 1 is a little big

    public OfferItemIndicatorsOverlay(
        IInputManager input,
        IEntityManager entMan,
        IEyeManager eye,
        OfferItemSystem offerSys
    )
    {
        _inputManager = input;
        _entMan = entMan;
        _eye = eye;
        _offer = offerSys;

        var spriteSys = _entMan.EntitySysManager.GetEntitySystem<SpriteSystem>();
        _sight = spriteSys.Frame0(
            new SpriteSpecifier.Rsi(
                new("/Textures/Interface/Misc/give_item.rsi"),
                "give_item"));
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_offer.IsInOfferMode())
            return false;

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var mouseScreenPosition = _inputManager.MouseScreenPosition;
        var mousePosMap = _eye.PixelToMap(mouseScreenPosition);
        if (mousePosMap.MapId != args.MapId)
            return;


        var mousePos = mouseScreenPosition.Position;
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var limitedScale = uiScale > 1.25f ? 1.25f : uiScale;

        DrawSight(_sight, args.ScreenHandle, mousePos, limitedScale * _scale);
    }

    private void DrawSight(Texture sight, DrawingHandleScreen screen, Vector2 centerPos, float scale)
    {
        var sightSize = sight.Size * scale;
        var expandedSize = sightSize + new Vector2(7f, 7f);

        screen.DrawTextureRect(
            sight,
            UIBox2.FromDimensions(centerPos - sightSize * 0.5f, sightSize),
            _strokeColor);
        screen.DrawTextureRect(
            sight,
            UIBox2.FromDimensions(centerPos - expandedSize * 0.5f, expandedSize),
            _mainColor);
    }
}
