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
    private readonly Button _button;

    public bool Preference
    {
        get => _button.Pressed;
        set => _button.Pressed = value;
    }

    public event Action<bool>? PreferenceChanged;

    public TraitPreferenceSelector(TraitPrototype trait, JobPrototype highJob,
        HumanoidCharacterProfile profile, string style, IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager, CharacterRequirementsSystem characterRequirementsSystem,
        JobRequirementsManager jobRequirementsManager)
    {
        Trait = trait;

        // Create a checkbox to get the loadout
        _button = new Button
        {
            VerticalAlignment = Control.VAlignment.Center,
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
                        new Label { Text = trait.Name },
                    },
                },
            },
        };
        _button.OnToggled += OnButtonToggled;
        _button.AddStyleClass(style);

        var tooltip = new StringBuilder();
        // Add the trait description to the tooltip if there is one
        if (Loc.TryGetString(trait.DescriptionLoc, out var desc))
            tooltip.Append(desc);


        // Get requirement reasons
        characterRequirementsSystem.CheckRequirementsValid(
            trait.Requirements, highJob, profile, new Dictionary<string, TimeSpan>(),
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
            Children = { _button },
        });
    }

    private void OnButtonToggled(BaseButton.ButtonToggledEventArgs args)
    {
        PreferenceChanged?.Invoke(Preference);
    }
}
