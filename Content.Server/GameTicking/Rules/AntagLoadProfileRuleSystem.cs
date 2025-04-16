using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Humanoid;
using Content.Server.Preferences.Managers;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules;

public sealed class AntagLoadProfileRuleSystem : GameRuleSystem<AntagLoadProfileRuleComponent>
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagLoadProfileRuleComponent, AntagSelectEntityEvent>(OnSelectEntity);
    }

    private void OnSelectEntity(Entity<AntagLoadProfileRuleComponent> ent, ref AntagSelectEntityEvent args)
    {
        if (args.Handled)
            return;

        var profile = args.Session != null
            ? _prefs.GetPreferences(args.Session.UserId).SelectedCharacter as HumanoidCharacterProfile
            : HumanoidCharacterProfile.RandomWithSpecies();

        SpeciesPrototype? species;
        if (ent.Comp.SpeciesOverride != null)
        {
            // Only override if species is in blacklist
            var useOverride = false;
            if (ent.Comp.SpeciesOverrideBlacklist != null && 
                profile?.Species is { } speciesId &&
                ent.Comp.SpeciesOverrideBlacklist.Contains(speciesId))
            {
                useOverride = true;
            }
            
            species = useOverride 
                ? _proto.Index(ent.Comp.SpeciesOverride.Value)
                : (profile?.Species is { } id && _proto.TryIndex(id, out SpeciesPrototype? profileSpecies))
                    ? profileSpecies
                    : _proto.Index<SpeciesPrototype>(SharedHumanoidAppearanceSystem.DefaultSpecies);
        }
        else if (profile?.Species is not { } speciesId || !_proto.TryIndex(speciesId, out species))
        {
            species = _proto.Index<SpeciesPrototype>(SharedHumanoidAppearanceSystem.DefaultSpecies);
        }

        args.Entity = Spawn(species.Prototype);
        if (profile != null)
        {
            _humanoid.LoadProfile(args.Entity.Value, profile.WithSpecies(species.ID));
        }
    }
}
