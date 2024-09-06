using Content.Client.Players.PlayTimeTracking;
using Content.Shared.Customization.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client.Preferences.UI;

public sealed class AntagPreferenceSelector : RequirementsSelector<AntagPrototype>
{
    // 0 is yes and 1 is no
    public bool Preference
    {
        get => Options.SelectedValue == 0;
        set => Options.Select(value && !Disabled ? 0 : 1);
    }

    public event Action<bool>? PreferenceChanged;

    public AntagPreferenceSelector(AntagPrototype proto, JobPrototype highJob) : base(proto, highJob)
    {
        Options.OnItemSelected += _ => PreferenceChanged?.Invoke(Preference);

        var items = new[]
        {
            ("humanoid-profile-editor-antag-preference-yes-button", 0),
            ("humanoid-profile-editor-antag-preference-no-button", 1),
        };
        var title = Loc.GetString(proto.Name);
        var description = Loc.GetString(proto.Objective);
        Setup(items, title, 250, description);

        // Immediately lock requirements if they aren't met.
        // Another function checks Disabled after creating the selector so this has to be done now
        var requirements = IoCManager.Resolve<JobRequirementsManager>();
        var prefs = IoCManager.Resolve<IClientPreferencesManager>();
        var entMan = IoCManager.Resolve<IEntityManager>();
        var characterReqs = entMan.System<CharacterRequirementsSystem>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var configMan = IoCManager.Resolve<IConfigurationManager>();

        if (proto.Requirements != null
            && !characterReqs.CheckRequirementsValid(
                proto.Requirements,
                highJob,
                (HumanoidCharacterProfile) (prefs.Preferences?.SelectedCharacter ?? HumanoidCharacterProfile.DefaultWithSpecies()),
                requirements.GetRawPlayTimeTrackers(),
                requirements.IsWhitelisted(),
                proto,
                entMan,
                protoMan,
                configMan,
                out var reasons))
            LockRequirements(characterReqs.GetRequirementsText(reasons));
    }
}
