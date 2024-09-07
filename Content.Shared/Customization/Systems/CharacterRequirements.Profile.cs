using System.Linq;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


/// <summary>
///     Requires the profile to be within an age range
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterAgeRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public int Min;

    [DataField(required: true)]
    public int Max;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-age-requirement",
            ("inverted", Inverted), ("min", Min), ("max", Max)));
        return profile.Age >= Min && profile.Age <= Max;
    }
}

/// <summary>
///   Requires the profile to use either a Backpack, Satchel, or Duffelbag
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterBackpackTypeRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public BackpackPreference Preference;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-backpack-type-requirement",
            ("inverted", Inverted),
            ("type", Loc.GetString($"humanoid-profile-editor-preference-{Preference.ToString().ToLower()}"))));
        return profile.Backpack == Preference;
    }
}

/// <summary>
///     Requires the profile to use either Jumpsuits or Jumpskirts
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterClothingPreferenceRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public ClothingPreference Preference;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-clothing-preference-requirement",
            ("inverted", Inverted),
            ("preference", Loc.GetString($"humanoid-profile-editor-preference-{Preference.ToString().ToLower()}"))));
        return profile.Clothing == Preference;
    }
}

/// <summary>
///     Requires the profile to be a certain gender
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterGenderRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public Gender Gender;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-gender-requirement",
            ("inverted", Inverted),
            ("gender", Loc.GetString($"humanoid-profile-editor-pronouns-{Gender.ToString().ToLower()}-text"))));
        return profile.Gender == Gender;
    }
}

/// <summary>
///     Requires the profile to be a certain sex
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterSexRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public Sex Sex;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-sex-requirement",
            ("inverted", Inverted),
            ("sex", Loc.GetString($"humanoid-profile-editor-sex-{Sex.ToString().ToLower()}-text"))));
        return profile.Sex == Sex;
    }
}

/// <summary>
///     Requires the profile to be a certain species
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterSpeciesRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<SpeciesPrototype>> Species;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        const string color = "green";
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-species-requirement",
            ("inverted", Inverted),
            ("species", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Species.Select(s => Loc.GetString(prototypeManager.Index(s).Name)))}[/color]")));

        return Species.Contains(profile.Species);
    }
}

/// <summary>
///    Requires the profile to be within a certain height range
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterHeightRequirement : CharacterRequirement
{
    /// <summary>
    ///     The minimum height of the profile in centimeters
    /// </summary>
    [DataField]
    public float Min = int.MinValue;

    /// <summary>
    ///     The maximum height of the profile in centimeters
    /// </summary>
    [DataField]
    public float Max = int.MaxValue;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        const string color = "yellow";
        var species = prototypeManager.Index<SpeciesPrototype>(profile.Species);

        reason = FormattedMessage.FromMarkup(Loc.GetString("character-height-requirement",
            ("inverted", Inverted), ("color", color), ("min", Min), ("max", Max)));

        var height = profile.Height * species.AverageHeight;
        return height >= Min && height <= Max;
    }
}

/// <summary>
///     Requires the profile to be within a certain width range
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterWidthRequirement : CharacterRequirement
{
    /// <summary>
    ///     The minimum width of the profile in centimeters
    /// </summary>
    [DataField]
    public float Min = int.MinValue;

    /// <summary>
    ///     The maximum width of the profile in centimeters
    /// </summary>
    [DataField]
    public float Max = int.MaxValue;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        const string color = "yellow";
        var species = prototypeManager.Index<SpeciesPrototype>(profile.Species);

        reason = FormattedMessage.FromMarkup(Loc.GetString("character-width-requirement",
            ("inverted", Inverted), ("color", color), ("min", Min), ("max", Max)));

        var width = profile.Width * species.AverageWidth;
        return width >= Min && width <= Max;
    }
}

/// <summary>
///     Requires the profile to be within a certain weight range
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterWeightRequirement : CharacterRequirement
{
    /// <summary>
    ///     Minimum weight of the profile in kilograms
    /// </summary>
    [DataField]
    public float Min = int.MinValue;

    /// <summary>
    ///     Maximum weight of the profile in kilograms
    /// </summary>
    [DataField]
    public float Max = int.MaxValue;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        const string color = "green";
        var species = prototypeManager.Index<SpeciesPrototype>(profile.Species);
        prototypeManager.Index(species.Prototype).TryGetComponent<FixturesComponent>(out var fixture);

        if (fixture == null)
        {
            reason = null;
            return false;
        }

        var weight = MathF.Round(
            MathF.PI * MathF.Pow(
                fixture.Fixtures["fix1"].Shape.Radius
                * ((profile.Width + profile.Height) / 2), 2)
                * fixture.Fixtures["fix1"].Density);

        reason = FormattedMessage.FromMarkup(Loc.GetString("character-weight-requirement",
            ("inverted", Inverted), ("color", color), ("min", Min), ("max", Max)));

        return weight >= Min && weight <= Max;
    }
}


/// <summary>
///     Requires the profile to have one of the specified traits
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterTraitRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<TraitPrototype>> Traits;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        const string color = "lightblue";
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-trait-requirement",
            ("inverted", Inverted),
            ("traits", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Traits.Select(t => Loc.GetString($"trait-name-{t}")))}[/color]")));

        return Traits.Any(t => profile.TraitPreferences.Contains(t.ToString()));
    }
}

/// <summary>
///     Requires the profile to have one of the specified loadouts
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterLoadoutRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<LoadoutPrototype>> Loadouts;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        const string color = "lightblue";
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-loadout-requirement",
            ("inverted", Inverted),
            ("loadouts", $"[color={color}]{string.Join($"[/color], [color={color}]",
                Loadouts.Select(l => Loc.GetString($"loadout-name-{l}")))}[/color]")));

        return Loadouts.Any(l => profile.LoadoutPreferences.Contains(l.ToString()));
    }
}

/// <summary>
///     Requires the profile to not have any more than X of the specified traits, loadouts, etc, in a group
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterItemGroupRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public ProtoId<CharacterItemGroupPrototype> Group;

    public override bool IsValid(JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason, int depth = 0)
    {
        var group = prototypeManager.Index(Group);

        // Get the count of items in the group that are in the profile
        var items = group.Items.Select(item => item.TryGetValue(profile, prototypeManager, out _) ? item.ID : null).Where(id => id != null).ToList();
        var count = items.Count;

        // If prototype is selected, remove one from the count
        if (items.ToList().Contains(prototype.ID))
            count--;

        reason = FormattedMessage.FromMarkup(Loc.GetString("character-item-group-requirement",
            ("inverted", Inverted),
            ("group", Loc.GetString($"character-item-group-{Group}")),
            ("max", group.MaxItems)));

        return count < group.MaxItems;
    }
}
