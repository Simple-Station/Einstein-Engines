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

    // Used to clearing on state changing
    private readonly HashSet<RadialContainer> _cachedContainers = new();

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
            ClearExistingContainers();
            CreateMenu(radialSelectorState.Entries);
            _openCentered = radialSelectorState.OpenCentered;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _menu.Dispose();
    }

    private void CreateMenu(List<RadialSelectorEntry> entries, string parentCategory = "")
    {
        var container = new RadialContainer
        {
            Name = !string.IsNullOrEmpty(parentCategory) ? parentCategory : "Main",
            Radius = 48f + 24f * MathF.Log(entries.Count),
        };

        _menu.AddChild(container);
        _cachedContainers.Add(container);

        foreach (var entry in entries)
        {
            if (entry.Category != null)
            {
                var button = CreateButton(entry.Category.Name, _spriteSystem.Frame0(entry.Category.Icon));
                button.TargetLayer = entry.Category.Name;
                CreateMenu(entry.Category.Entries, entry.Category.Name);
                container.AddChild(button);
            }
            else if (entry.Prototype != null && _protoManager.TryIndex(entry.Prototype, out var proto))
            {
                var button = CreateButton(proto.Name, _spriteSystem.Frame0(proto));
                button.OnButtonUp += _ =>
                {
                    var msg = new RadialSelectorSelectedMessage(entry.Prototype);
                    SendMessage(msg);
                    Close();
                };

                container.AddChild(button);
            }
        }
    }

    private RadialMenuTextureButton CreateButton(string name, Texture icon)
    {
        var itemSize = new Vector2(64f, 64f);
        var button = new RadialMenuTextureButton
        {
            ToolTip = Loc.GetString(name),
            StyleClasses = { "RadialMenuButton" },
            SetSize = itemSize
        };

        var iconScale = itemSize / icon.Size;
        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = icon,
            TextureScale = iconScale
        };

        button.AddChild(texture);
        return button;
    }

    private void ClearExistingContainers()
    {
        foreach (var container in _cachedContainers)
        {
            _menu.RemoveChild(container);
        }

        _cachedContainers.Clear();
    }
}
