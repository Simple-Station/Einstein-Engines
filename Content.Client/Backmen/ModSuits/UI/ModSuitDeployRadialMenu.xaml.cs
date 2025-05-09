using System.Linq;
using Content.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using System.Numerics;
using Content.Shared.Backmen.ModSuits;

namespace Content.Client.Backmen.ModSuits.UI;

public sealed partial class ModSuitDeployRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    private readonly SharedAppearanceSystem _appearanceSystem;
    public event Action<EntityUid>? SendToggleClothingMessageAction;

    public EntityUid Entity { get; set; }
    public List<EntityUid> PreviewEntities { get; set; } = new();

    public ModSuitDeployRadialMenu()
    {
        IoCManager.InjectDependencies(this);

        _appearanceSystem = _entityManager.System<SharedAppearanceSystem>();

        RobustXamlLoader.Load(this);
    }

    public void SetEntity(EntityUid uid)
    {
        Entity = uid;
        RefreshUI();
    }

    public void RefreshUI()
    {
        var main = FindControl<RadialContainer>("Main");

        if (!_appearanceSystem.TryGetData<ModSuitVisualizerGroupData>(Entity, ModSuitVisualizerKeys.ClothingPieces, out var clothingContainer))
            return;

        foreach (var attached in clothingContainer.PieceList.Select(_entityManager.GetEntity))
        {
            // Change tooltip text if attached clothing is toggle/untoggled
            var tooltipText = Loc.GetString("modsuit-unattach-tooltip");

            if (clothingContainer.AttachedPieces.Select(_entityManager.GetEntity).Contains(attached))
                tooltipText = Loc.GetString("modsuit-attach-tooltip");

            var button = new ModSuitRadialMenuButton
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64, 64),
                ToolTip = tooltipText,
                AttachedClothingId = attached,
            };

            var spriteView = new SpriteView
            {
                SetSize = new Vector2(48, 48),
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Stretch = SpriteView.StretchMode.Fill,
            };

            // If the attached entity is in a container, the player can't see the icon. Very ba
            var entView = _entityManager.Spawn(_entityManager.GetComponent<MetaDataComponent>(attached).EntityPrototype!.ID);
            PreviewEntities.Add(entView);

            spriteView.SetEntity(entView);

            button.AddChild(spriteView);
            main.AddChild(button);
        }

        AddModSuitMenuButtonOnClickAction(main);
    }

    private void AddModSuitMenuButtonOnClickAction(Control control)
    {
        if (control is not RadialContainer mainControl)
            return;

        foreach (var castChild in mainControl.Children.Select(child => child as ModSuitRadialMenuButton))
        {
            if (castChild == null)
                return;

            castChild.OnButtonDown += _ =>
            {
                SendToggleClothingMessageAction?.Invoke(castChild.AttachedClothingId);
                mainControl.DisposeAllChildren();

                // clean up the previews
                foreach (var ent in PreviewEntities)
                {
                    _entityManager.QueueDeleteEntity(ent);
                }
                PreviewEntities.Clear();

                RefreshUI();
            };
        }
    }
}

public sealed class ModSuitRadialMenuButton : RadialMenuTextureButton
{
    public EntityUid AttachedClothingId { get; set; }
}
