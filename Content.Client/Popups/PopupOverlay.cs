using System.Numerics;
using Content.Shared.Examine;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Popups;

/// <summary>
/// Draws popup text, either in world or on screen.
/// </summary>
public sealed class PopupOverlay : Overlay
{
    private readonly IConfigurationManager _configManager;
    private readonly IEntityManager _entManager;
    private readonly IPlayerManager _playerMgr;
    private readonly IUserInterfaceManager _uiManager;
    private readonly PopupSystem _popup;
    private readonly PopupUIController _controller;
    private readonly ExamineSystemShared _examine;
    private readonly SharedTransformSystem _transform;
    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public PopupOverlay(
        IConfigurationManager configManager,
        IEntityManager entManager,
        IPlayerManager playerMgr,
        IPrototypeManager protoManager,
        IUserInterfaceManager uiManager,
        PopupUIController controller,
        ExamineSystemShared examine,
        SharedTransformSystem transform,
        PopupSystem popup)
    {
        _configManager = configManager;
        _entManager = entManager;
        _playerMgr = playerMgr;
        _uiManager = uiManager;
        _examine = examine;
        _transform = transform;
        _popup = popup;
        _controller = controller;

        _shader = protoManager.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null)
            return;

        args.DrawingHandle.SetTransform(Matrix3x2.Identity);
        args.DrawingHandle.UseShader(_shader);
        var scale = _configManager.GetCVar(CVars.DisplayUIScale);

        if (scale == 0f)
            scale = _uiManager.DefaultUIScale;

        DrawWorld(args.ScreenHandle, args, scale);

        args.DrawingHandle.UseShader(null);
    }

    private void DrawWorld(DrawingHandleScreen worldHandle, OverlayDrawArgs args, float scale)
    {
        if (_popup.WorldLabels.Count == 0 || args.ViewportControl == null)
            return;

        var matrix = args.ViewportControl.GetWorldToScreenMatrix();
        var viewPos = new MapCoordinates(args.WorldAABB.Center, args.MapId);
        var ourEntity = _playerMgr.LocalEntity;

        foreach (var popup in _popup.WorldLabels)
        {
            var mapPos = popup.InitialPos.ToMap(_entManager, _transform);

            if (mapPos.MapId != args.MapId)
                continue;

            var distance = (mapPos.Position - args.WorldBounds.Center).Length();

            // Should handle fade here too wyci.
            if (!args.WorldBounds.Contains(mapPos.Position) || !_examine.InRangeUnOccluded(viewPos, mapPos, distance,
                    e => e == popup.InitialPos.EntityId || e == ourEntity, entMan: _entManager))
                continue;

            var pos = Vector2.Transform(mapPos.Position, matrix);
            _controller.DrawPopup(popup, worldHandle, pos, scale);
        }
    }
<<<<<<< HEAD
||||||| parent of 7fe67c7209 (Blob try 2 (#176))

    private void DrawScreen(DrawingHandleScreen screenHandle, OverlayDrawArgs args, float scale)
    {
        foreach (var popup in _popup.CursorLabels)
        {
            // Different window
            if (popup.InitialPos.Window != args.ViewportControl?.Window?.Id)
                continue;

            DrawPopup(popup, screenHandle, popup.InitialPos.Position, scale);
        }
    }

    private void DrawPopup(PopupSystem.PopupLabel popup, DrawingHandleScreen handle, Vector2 position, float scale)
    {
        var lifetime = PopupSystem.GetPopupLifetime(popup);

        // Keep alpha at 1 until TotalTime passes half its lifetime, then gradually decrease to 0.
        var alpha = MathF.Min(1f, 1f - MathF.Max(0f, popup.TotalTime - lifetime / 2) * 2 / lifetime);

        var updatedPosition = position - new Vector2(0f, MathF.Min(8f, 12f * (popup.TotalTime * popup.TotalTime + popup.TotalTime)));
        var font = _smallFont;
        var color = Color.White.WithAlpha(alpha);

        switch (popup.Type)
        {
            case PopupType.SmallCaution:
                color = Color.Red;
                break;
            case PopupType.Medium:
                font = _mediumFont;
                color = Color.LightGray;
                break;
            case PopupType.MediumCaution:
                font = _mediumFont;
                color = Color.Red;
                break;
            case PopupType.Large:
                font = _largeFont;
                color = Color.LightGray;
                break;
            case PopupType.LargeCaution:
                font = _largeFont;
                color = Color.Red;
                break;
        }

        var dimensions = handle.GetDimensions(font, popup.Text, scale);
        handle.DrawString(font, updatedPosition - dimensions / 2f, popup.Text, scale, color.WithAlpha(alpha));
    }
=======

    private void DrawScreen(DrawingHandleScreen screenHandle, OverlayDrawArgs args, float scale)
    {
        foreach (var popup in _popup.CursorLabels)
        {
            // Different window
            if (popup.InitialPos.Window != args.ViewportControl?.Window?.Id)
                continue;

            DrawPopup(popup, screenHandle, popup.InitialPos.Position, scale);
        }
    }

    private void DrawPopup(PopupSystem.PopupLabel popup, DrawingHandleScreen handle, Vector2 position, float scale)
    {
        var lifetime = PopupSystem.GetPopupLifetime(popup);

        // Keep alpha at 1 until TotalTime passes half its lifetime, then gradually decrease to 0.
        var alpha = MathF.Min(1f, 1f - MathF.Max(0f, popup.TotalTime - lifetime / 2) * 2 / lifetime);

        var updatedPosition = position - new Vector2(0f, MathF.Min(8f, 12f * (popup.TotalTime * popup.TotalTime + popup.TotalTime)));
        var font = _smallFont;
        var color = Color.White.WithAlpha(alpha);

        switch (popup.Type)
        {
            case PopupType.SmallCaution:
                color = Color.Red;
                break;
            case PopupType.Medium:
                font = _mediumFont;
                color = Color.LightGray;
                break;
            case PopupType.MediumCaution:
                font = _mediumFont;
                color = Color.Red;
                break;
            case PopupType.Large:
                font = _largeFont;
                color = Color.LightGray;
                break;
            case PopupType.LargeCaution:
                font = _largeFont;
                color = Color.Red;
                break;
            case PopupType.LargeGreen:
                font = _largeFont;
                color = Color.LightGreen;
                break;
        }

        var dimensions = handle.GetDimensions(font, popup.Text, scale);
        handle.DrawString(font, updatedPosition - dimensions / 2f, popup.Text, scale, color.WithAlpha(alpha));
    }
>>>>>>> 7fe67c7209 (Blob try 2 (#176))
}
