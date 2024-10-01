using System.Text;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.Stylesheets;
using Content.Shared.Customization.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Preferences.UI;


public sealed class TraitPreferenceSelector : Control
{
    public TraitPrototype Trait { get; }

    public bool Valid;
    private bool _showUnusable;
    public bool ShowUnusable
    {
        get => _showUnusable;
        set
        {
            _showUnusable = value;
            Visible = Valid || _showUnusable;
            PreferenceButton.RemoveStyleClass(StyleBase.ButtonDanger);
            PreferenceButton.AddStyleClass(Valid ? "" : StyleBase.ButtonDanger);
        }
    }

    public Button PreferenceButton;
    public bool Preference
    {
        get => PreferenceButton.Pressed;
        set => PreferenceButton.Pressed = value;
    }

    public event Action<bool>? PreferenceChanged;

    public TraitPreferenceSelector(TraitPrototype trait, JobPrototype highJob, HumanoidCharacterProfile profile,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        CharacterRequirementsSystem characterRequirementsSystem, JobRequirementsManager jobRequirementsManager)
    {
        Trait = trait;

        // Create a checkbox to get the loadout
        PreferenceButton = new Button
        {
            VerticalAlignment = VAlignment.Center,
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
                            Text = trait.Points.ToString(),
                            StyleClasses = { StyleBase.StyleClassLabelHeading },
                            MinWidth = 32,
                            MaxWidth = 32,
                            ClipText = true,
                            Margin = new Thickness(0, 0, 8, 0),
                        },
                        new Label { Text = Loc.GetString($"trait-name-{trait.ID}") },
                    },
                },
            },
        };
        PreferenceButton.OnToggled += OnPreferenceButtonToggled;

        var tooltip = new StringBuilder();
        // Add the loadout description to the tooltip if there is one
        var desc = Loc.GetString($"trait-description-{trait.ID}");
        if (!string.IsNullOrEmpty(desc) && desc != $"trait-description-{trait.ID}")
            tooltip.Append(desc);


        // Get requirement reasons
        characterRequirementsSystem.CheckRequirementsValid(
            trait.Requirements, highJob, profile, new Dictionary<string, TimeSpan>(),
            jobRequirementsManager.IsWhitelisted(), trait,
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
