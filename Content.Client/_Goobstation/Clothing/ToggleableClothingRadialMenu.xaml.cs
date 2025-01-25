using Content.Client.UserInterface.Controls;
using Content.Shared.Clothing.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Client._Goobstation.Clothing;

public sealed partial class ToggleableClothingRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    public event Action<EntityUid>? SendToggleClothingMessageAction;

    public EntityUid Entity { get; set; }

    public ToggleableClothingRadialMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
    }

    public void SetEntity(EntityUid uid)
    {
        Entity = uid;
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Even EmotesMenu has to call this, I'm assuming it's essential.
        var main = FindControl<RadialContainer>("Main");

        if (!_entityManager.TryGetComponent<ToggleableClothingComponent>(Entity, out var clothing)
            || clothing.Container is not { } clothingContainer)
            return;

        foreach (var attached in clothing.ClothingUids)
        {
            // Change tooltip text if attached clothing is toggle/untoggled
            var tooltipText = Loc.GetString(clothing.UnattachTooltip);

            if (clothingContainer.Contains(attached.Key))
                tooltipText = Loc.GetString(clothing.AttachTooltip);

            var button = new ToggleableClothingRadialMenuButton()
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
                ToolTip = tooltipText,
                AttachedClothingId = attached.Key
            };

            var spriteView = new SpriteView()
            {
                SetSize = new Vector2(48, 48),
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Stretch = SpriteView.StretchMode.Fill
            };

            spriteView.SetEntity(attached.Key);

            button.AddChild(spriteView);
            main.AddChild(button);
        }

        AddToggleableClothingMenuButtonOnClickAction(main);
    }

    private void AddToggleableClothingMenuButtonOnClickAction(Control control)
    {
        if (control is not RadialContainer mainControl)
            return;

        foreach (var child in mainControl.Children)
        {
            if (child is not ToggleableClothingRadialMenuButton castChild)
                continue;

            castChild.OnButtonDown += _ =>
            {
                SendToggleClothingMessageAction?.Invoke(castChild.AttachedClothingId);
                mainControl.DisposeAllChildren();
                RefreshUI();
            };
        }
    }
}

public sealed class ToggleableClothingRadialMenuButton : RadialMenuTextureButton
{
    public EntityUid AttachedClothingId { get; set; }
}
