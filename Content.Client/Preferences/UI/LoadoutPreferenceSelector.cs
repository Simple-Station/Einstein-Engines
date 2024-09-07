using System.Linq;
using System.Numerics;
using System.Text;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.Stylesheets;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Customization.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Client.Graphics;
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

    public bool Valid;
    private bool _showUnusable;
    public bool ShowUnusable
    {
        get => _showUnusable;
        set
        {
            _showUnusable = value;
            Visible = Valid && _wearable || _showUnusable;
            PreferenceButton.RemoveStyleClass(StyleBase.ButtonDanger);
            PreferenceButton.AddStyleClass(Valid ? "" : StyleBase.ButtonDanger);
        }
    }

    private bool _wearable;
    public bool Wearable
    {
        get => _wearable;
        set
        {
            _wearable = value;
            Visible = Valid && _wearable || _showUnusable;
            PreferenceButton.RemoveStyleClass(StyleBase.ButtonCaution);
            PreferenceButton.AddStyleClass(_wearable ? "" : StyleBase.ButtonCaution);
        }
    }

    public Button PreferenceButton;
    public bool Preference
    {
        get => PreferenceButton.Pressed;
        set => PreferenceButton.Pressed = value;
    }

    public event Action<bool>? PreferenceChanged;

    public LoadoutPreferenceSelector(LoadoutPrototype loadout, JobPrototype highJob,
        HumanoidCharacterProfile profile, ref Dictionary<string, EntityUid> entities,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        CharacterRequirementsSystem characterRequirementsSystem, JobRequirementsManager jobRequirementsManager)
    {
        Loadout = loadout;

        SpriteView previewLoadout;
        if (!entities.TryGetValue(loadout.ID + 0, out var dummyLoadoutItem))
        {
            // Get the first item in the loadout to be the preview
            dummyLoadoutItem = entityManager.SpawnEntity(loadout.Items.First(), MapCoordinates.Nullspace);

            // Create a sprite preview of the loadout item
            previewLoadout = new SpriteView
            {
                Scale = new Vector2(1, 1),
                OverrideDirection = Direction.South,
                VerticalAlignment = VAlignment.Center,
                SizeFlagsStretchRatio = 1,
            };
            previewLoadout.SetEntity(dummyLoadoutItem);
        }
        else
        {
            // Create a sprite preview of the loadout item
            previewLoadout = new SpriteView
            {
                Scale = new Vector2(1, 1),
                OverrideDirection = Direction.South,
                VerticalAlignment = VAlignment.Center,
                SizeFlagsStretchRatio = 1,
            };
            previewLoadout.SetEntity(dummyLoadoutItem);
        }


        // Create a checkbox to get the loadout
        PreferenceButton = new Button
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
                        new PanelContainer
                        {
                            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#2f2f2f") },
                            Children =
                            {
                                previewLoadout,
                            },
                        },
                        new Label
                        {
                            Text = Loc.GetString($"loadout-name-{loadout.ID}") == $"loadout-name-{loadout.ID}"
                                ? entityManager.GetComponent<MetaDataComponent>(dummyLoadoutItem).EntityName
                                : Loc.GetString($"loadout-name-{loadout.ID}"),
                            Margin = new Thickness(8, 0, 0, 0),
                        },
                    },
                },
            },
        };
        PreferenceButton.OnToggled += OnPreferenceButtonToggled;

        var tooltip = new StringBuilder();
        // Add the loadout description to the tooltip if there is one
        var desc = !Loc.TryGetString($"loadout-description-{loadout.ID}", out var description)
            ? entityManager.GetComponent<MetaDataComponent>(dummyLoadoutItem).EntityDescription
            : description;
        if (!string.IsNullOrEmpty(desc))
            tooltip.Append($"{Loc.GetString(desc)}");


        // Get requirement reasons
        characterRequirementsSystem.CheckRequirementsValid(
            loadout.Requirements, highJob, profile, new Dictionary<string, TimeSpan>(),
            jobRequirementsManager.IsWhitelisted(), loadout,
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
            PreferenceButton.TooltipSupplier = _ => formattedTooltip;
        }


        // Add the loadout preview and the checkbox to the control
        AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Children = { PreferenceButton },
        });
    }

    private void OnPreferenceButtonToggled(BaseButton.ButtonToggledEventArgs args)
    {
        PreferenceChanged?.Invoke(Preference);
    }
}
