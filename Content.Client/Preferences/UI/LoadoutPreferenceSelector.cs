using System.Linq;
using System.Numerics;
using System.Text;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.Stylesheets;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Customization.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Preferences.UI;


public sealed class LoadoutPreferenceSelector : Control
{
    public LoadoutPrototype Loadout { get; }
    private readonly Button _button;

    public bool Preference
    {
        get => _button.Pressed;
        set => _button.Pressed = value;
    }

    public event Action<bool>? PreferenceChanged;

    public LoadoutPreferenceSelector(LoadoutPrototype loadout, JobPrototype highJob,
        HumanoidCharacterProfile profile, string style, IEntityManager entityManager, IPrototypeManager prototypeManager,
        IConfigurationManager configManager, CharacterRequirementsSystem characterRequirementsSystem,
        JobRequirementsManager jobRequirementsManager)
    {
        Loadout = loadout;

        // Display the first item in the loadout as a preview
        // TODO: Maybe allow custom icons to be specified in the prototype?
        var dummyLoadoutItem = entityManager.SpawnEntity(loadout.Items.First(), MapCoordinates.Nullspace);

        // Create a sprite preview of the loadout item
        var previewLoadout = new SpriteView
        {
            Scale = new Vector2(1, 1),
            OverrideDirection = Direction.South,
            VerticalAlignment = VAlignment.Center,
            SizeFlagsStretchRatio = 1,
        };
        previewLoadout.SetEntity(dummyLoadoutItem);


        // Create a checkbox to get the loadout
        _button = new Button
        {
            ToggleMode = true,
            StyleClasses = { StyleBase.ButtonOpenLeft },
            Children =
            {
                new BoxContainer
                {
                    Children =
                    {
                        new Label
                        {
                            Text = loadout.Cost.ToString(),
                            StyleClasses = { StyleBase.StyleClassLabelHeading },
                            MinWidth = 32,
                            MaxWidth = 32,
                            ClipText = true,
                            Margin = new Thickness(0, 0, 8, 0),
                        },
                        new Label
                        {
                            Text = !Loc.TryGetString(loadout.NameLoc, out var name)
                                ? entityManager.GetComponent<MetaDataComponent>(dummyLoadoutItem).EntityName
                                : name,
                        },
                    },
                },
            },
        };
        _button.OnToggled += OnButtonToggled;
        _button.AddStyleClass(style);

        var tooltip = new StringBuilder();
        // Add the loadout description to the tooltip if there is one
        var desc = !Loc.TryGetString(loadout.DescriptionLoc, out var description)
            ? entityManager.GetComponent<MetaDataComponent>(dummyLoadoutItem).EntityDescription
            : description;
        if (!string.IsNullOrEmpty(desc))
            tooltip.Append($"{Loc.GetString(desc)}");


        // Get requirement reasons
        characterRequirementsSystem.CheckRequirementsValid(
            loadout.Requirements, highJob, profile, new Dictionary<string, TimeSpan>(),
            jobRequirementsManager.IsWhitelisted(),
            entityManager, prototypeManager, configManager,
            out var reasons);

        // Add requirement reasons to the tooltip
        foreach (var reason in reasons)
            tooltip.Append($"\n{reason.ToMarkup()}");

        // Combine the tooltip and format it in the checkbox supplier
        if (tooltip.Length > 0)
        {
            var formattedTooltip = new Tooltip();
            formattedTooltip.SetMessage(FormattedMessage.FromMarkupPermissive(tooltip.ToString()));
            _button.TooltipSupplier = _ => formattedTooltip;
        }


        // Add the loadout preview and the checkbox to the control
        AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Children = { previewLoadout, _button },
        });
    }

    private void OnButtonToggled(BaseButton.ButtonToggledEventArgs args)
    {
        PreferenceChanged?.Invoke(Preference);
    }
}
