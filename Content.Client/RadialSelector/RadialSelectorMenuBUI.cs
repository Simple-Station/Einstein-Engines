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

    public Action<string, int>? ItemSelected;

    private readonly SpriteSystem _spriteSystem;

    private readonly RadialMenu _menu = new()
    {
        HorizontalExpand = true,
        VerticalExpand = true,
        BackButtonStyleClass = "RadialMenuBackButton",
        CloseButtonStyleClass = "RadialMenuCloseButton"
    };

    private bool _openCentered;

    public RadialSelectorMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _spriteSystem = _entManager.System<SpriteSystem>();
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
        _menu.Children.Clear();

        var mainContainer = new RadialContainer
        {
            Radius = 36f / (items.Count == 1 ? 1 : MathF.Sin(MathF.PI / items.Count))
        };

        foreach (var protoId in items)
        {
            if (!_protoManager.TryIndex(protoId, out var proto))
                continue;

            var itemSize = new Vector2(64f, 64f);
            var button = new RadialMenuTextureButton
            {
                ToolTip = Loc.GetString(proto.Name),
                StyleClasses = { "RadialMenuButton" },
                SetSize = itemSize
            };
            var runeIcon = _spriteSystem.Frame0(proto);
            var runeScale = itemSize / runeIcon.Size;

            var texture = new TextureRect
            {
                VerticalAlignment = Control.VAlignment.Center,
                HorizontalAlignment = Control.HAlignment.Center,
                Texture = runeIcon,
                TextureScale = runeScale
            };

            button.AddChild(texture);

            button.OnButtonUp += _ =>
            {
                var msg = new RadialSelectorSelectedMessage(protoId);
                SendMessage(msg);
                Close();
            };

            mainContainer.AddChild(button);
        }

        _menu.AddChild(mainContainer);
    }
}
