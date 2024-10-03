using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.RadialSelector;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

// ReSharper disable InconsistentNaming

namespace Content.Client.RadialSelector;

[UsedImplicitly]
public sealed class RadialSelectorMenuBUI : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly EntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    private readonly SpriteSystem _spriteSystem;

    private readonly RadialMenu _menu;
    private readonly RadialContainer _mainContainer;

    private bool _openCentered;

    public RadialSelectorMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _spriteSystem = _entManager.System<SpriteSystem>();
        _menu = new RadialMenu
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            BackButtonStyleClass = "RadialMenuBackButton",
            CloseButtonStyleClass = "RadialMenuCloseButton"
        };

        _mainContainer = new RadialContainer
        {
            Radius = 64f
        };

        _menu.AddChild(_mainContainer);
    }

    protected override void Open()
    {
        _menu.OnClose += Close;

        if (_openCentered)
        {
            _menu.OpenCentered();
        }
        else
        {
            _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / _displayManager.ScreenSize);
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is RadialSelectorState radialSelectorState)
        {
            PopulateMenu(radialSelectorState.Items);
            _openCentered = radialSelectorState.OpenCentered;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _menu.Dispose();
    }

    private void PopulateMenu(List<EntProtoId> items)
    {
        _mainContainer.Children.Clear();
        _mainContainer.Radius = 48f + 24f * MathF.Log(items.Count);

        foreach (var protoId in items)
        {
            if (!_protoManager.TryIndex(protoId, out var proto))
                continue;

            var itemSize = new Vector2(48f, 48f);
            var button = new RadialMenuTextureButton
            {
                ToolTip = Loc.GetString(proto.Name),
                StyleClasses = { "RadialMenuButton" },
                SetSize = itemSize
            };
            var icon = _spriteSystem.Frame0(proto);
            var iconScale = itemSize / icon.Size;

            var texture = new TextureRect
            {
                VerticalAlignment = Control.VAlignment.Center,
                HorizontalAlignment = Control.HAlignment.Center,
                Texture = icon,
                TextureScale = iconScale
            };

            button.AddChild(texture);

            button.OnButtonUp += _ =>
            {
                var msg = new RadialSelectorSelectedMessage(protoId);
                SendMessage(msg);
                Close();
            };

            _mainContainer.AddChild(button);
        }
    }
}
