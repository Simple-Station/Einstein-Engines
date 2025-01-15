using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared.CCVar;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences;

/// Character profile. Looks immutable, but uses non-immutable semantics internally for serialization/code sanity purposes
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class HumanoidCharacterProfile : ICharacterProfile
{
    private static readonly Regex RestrictedNameRegex = new(@"[^A-Za-z0-9 '\-]");
    private static readonly Regex ICNameCaseRegex = new(@"^(?<word>\w)|\b(?<word>\w)(?=\w*$)");

    public const int MaxNameLength = 64;
    public const int MaxDescLength = 1024;

    /// Job preferences for initial spawn
    [DataField]
    private Dictionary<string, JobPriority> _jobPriorities = new()
    {
        {
            SharedGameTicker.FallbackOverflowJob, JobPriority.High
        },
    };

    /// Antags we have opted in to
    [DataField]
    private HashSet<string> _antagPreferences = new();

    /// Enabled traits
    [DataField]
    private HashSet<string> _traitPreferences = new();

    /// <see cref="_loadoutPreferences"/>
    public HashSet<LoadoutPreference> LoadoutPreferences => _loadoutPreferences;

    [DataField]
    private HashSet<LoadoutPreference> _loadoutPreferences = new();

    [DataField]
    public string Name { get; set; } = "John Doe";

    /// Detailed text that can appear for the character if <see cref="CCVars.FlavorText"/> is enabled
    [DataField]
    public string FlavorText { get; set; } = string.Empty;

    /// Associated <see cref="SpeciesPrototype"/> for this profile
    [DataField]
    public string Species { get; set; } = SharedHumanoidAppearanceSystem.DefaultSpecies;

    [DataField]
    public string Customspeciename { get; set; } = "";

    [DataField]
    public float Height { get; private set; }

    [DataField]
    public float Width { get; private set; }

    [DataField]
    public int Age { get; set; } = 18;

    [DataField]
    public Sex Sex { get; private set; } = Sex.Male;

    [DataField]
    public Gender Gender { get; private set; } = Gender.Male;

    [DataField]
    public string? DisplayPronouns { get; set; }

    [DataField]
    public string? StationAiName { get; set; }

    [DataField]
    public string? CyborgName { get; set; }

    /// <see cref="Appearance"/>
    public ICharacterAppearance CharacterAppearance => Appearance;

    /// Stores markings, eye colors, etc for the profile
    [DataField]
    public HumanoidCharacterAppearance Appearance { get; set; } = new();

    [DataField]
    public ClothingPreference Clothing { get; set; }
    [DataField]
    public BackpackPreference Backpack { get; set; }

    /// When spawning into a round what's the preferred spot to spawn
    [DataField]
    public SpawnPriorityPreference SpawnPriority { get; private set; } = SpawnPriorityPreference.None;

    /// <see cref="_jobPriorities"/>
    public IReadOnlyDictionary<string, JobPriority> JobPriorities => _jobPriorities;

    /// <see cref="_antagPreferences"/>
    public IReadOnlySet<string> AntagPreferences => _antagPreferences;

    /// <see cref="_traitPreferences"/>
    public IReadOnlySet<string> TraitPreferences => _traitPreferences;

    /// If we're unable to get one of our preferred jobs do we spawn as a fallback job or do we stay in lobby
    [DataField]
    public PreferenceUnavailableMode PreferenceUnavailable { get; private set; } =
        PreferenceUnavailableMode.SpawnAsOverflow;

    public HumanoidCharacterProfile(
        string name,
        string flavortext,
        string species,
        string customspeciename,
        float height,
        float width,
        int age,
        Sex sex,
        Gender gender,
        string? displayPronouns,
        string? stationAiName,
        string? cyborgName,
        HumanoidCharacterAppearance appearance,
        SpawnPriorityPreference spawnPriority,
        Dictionary<string, JobPriority> jobPriorities,
        ClothingPreference clothing,
        BackpackPreference backpack,
        PreferenceUnavailableMode preferenceUnavailable,
        HashSet<string> antagPreferences,
        HashSet<string> traitPreferences,
        HashSet<LoadoutPreference> loadoutPreferences)
    {
        Name = name;
        FlavorText = flavortext;
        Species = species;
        Customspeciename = customspeciename;
        Height = height;
        Width = width;
        Age = age;
        Sex = sex;
        Gender = gender;
        DisplayPronouns = displayPronouns;
        StationAiName = stationAiName;
        CyborgName = cyborgName;
        Appearance = appearance;
        SpawnPriority = spawnPriority;
        _jobPriorities = jobPriorities;
        Clothing = clothing;
        Backpack = backpack;
        PreferenceUnavailable = preferenceUnavailable;
        _antagPreferences = antagPreferences;
        _traitPreferences = traitPreferences;
        _loadoutPreferences = loadoutPreferences;
    }

    /// <summary>Copy constructor</summary>
    public HumanoidCharacterProfile(HumanoidCharacterProfile other)
        : this(
            other.Name,
            other.FlavorText,
            other.Species,
            other.Customspeciename,
            other.Height,
            other.Width,
            other.Age,
            other.Sex,
            other.Gender,
            other.DisplayPronouns,
            other.StationAiName,
            other.CyborgName,
            other.Appearance.Clone(),
            other.SpawnPriority,
            new Dictionary<string, JobPriority>(other.JobPriorities),
            other.Clothing,
            other.Backpack,
            other.PreferenceUnavailable,
            new HashSet<string>(other.AntagPreferences),
            new HashSet<string>(other.TraitPreferences),
            new HashSet<LoadoutPreference>(other.LoadoutPreferences))
    {
    }

    /// <summary>
    ///     Get the default humanoid character profile, using internal constant values.
    ///     Defaults to <see cref="SharedHumanoidAppearanceSystem.DefaultSpecies"/> for the species.
    /// </summary>
    /// <returns></returns>
    public HumanoidCharacterProfile()
    {
    }

    /// <summary>
    ///     Return a default character profile, based on species.
    /// </summary>
    /// <param name="species">The species to use in this default profile. The default species is <see cref="SharedHumanoidAppearanceSystem.DefaultSpecies"/>.</param>
    /// <returns>Humanoid character profile with default settings.</returns>
    public static HumanoidCharacterProfile DefaultWithSpecies(string species = SharedHumanoidAppearanceSystem.DefaultSpecies)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var skinColor = SkinColor.ValidHumanSkinTone;

        if (prototypeManager.TryIndex<SpeciesPrototype>(species, out var speciesPrototype))
            skinColor = speciesPrototype.DefaultSkinTone;

        return new()
        {
            Species = species,
            Appearance = new()
            {
                SkinColor = skinColor,
            },
        };
    }

    // TODO: This should eventually not be a visual change only.
    public static HumanoidCharacterProfile Random(HashSet<string>? ignoredSpecies = null)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var random = IoCManager.Resolve<IRobustRandom>();

        var species = random.Pick(prototypeManager
            .EnumeratePrototypes<SpeciesPrototype>()
            .Where(x => ignoredSpecies == null ? x.RoundStart : x.RoundStart && !ignoredSpecies.Contains(x.ID))
            .ToArray()
        ).ID;

        return RandomWithSpecies(species);
    }

    public static HumanoidCharacterProfile RandomWithSpecies(string species = SharedHumanoidAppearanceSystem.DefaultSpecies)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var random = IoCManager.Resolve<IRobustRandom>();

        var sex = Sex.Unsexed;
        var age = 18;
        if (prototypeManager.TryIndex<SpeciesPrototype>(species, out var speciesPrototype))
        {
            sex = random.Pick(speciesPrototype.Sexes);
            age = random.Next(speciesPrototype.MinAge, speciesPrototype.OldAge); // people don't look and keep making 119 year old characters with zero rp, cap it at middle aged
        }

        var gender = Gender.Epicene;

        switch (sex)
        {
            case Sex.Male:
                gender = Gender.Male;
                break;
            case Sex.Female:
                gender = Gender.Female;
                break;
        }

        var name = GetName(species, gender);

        return new HumanoidCharacterProfile()
        {
            Name = name,
            Sex = sex,
            Age = age,
            Gender = gender,
            Species = species,
            Appearance = HumanoidCharacterAppearance.Random(species, sex),
        };
    }

    public HumanoidCharacterProfile WithName(string name) => new(this) { Name = name };
    public HumanoidCharacterProfile WithFlavorText(string flavorText) => new(this) { FlavorText = flavorText };
    public HumanoidCharacterProfile WithAge(int age) => new(this) { Age = age };
    public HumanoidCharacterProfile WithSex(Sex sex) => new(this) { Sex = sex };
    public HumanoidCharacterProfile WithGender(Gender gender) => new(this) { Gender = gender };
    public HumanoidCharacterProfile WithDisplayPronouns(string? displayPronouns) => new(this) { DisplayPronouns = displayPronouns };
    public HumanoidCharacterProfile WithStationAiName(string? stationAiName) => new(this) { StationAiName = stationAiName };
    public HumanoidCharacterProfile WithCyborgName(string? cyborgName) => new(this) { CyborgName = cyborgName };
    public HumanoidCharacterProfile WithSpecies(string species) => new(this) { Species = species };
    public HumanoidCharacterProfile WithCustomSpeciesName(string customspeciename) => new(this) { Customspeciename = customspeciename };
    public HumanoidCharacterProfile WithHeight(float height) => new(this) { Height = height };
    public HumanoidCharacterProfile WithWidth(float width) => new(this) { Width = width };

    public HumanoidCharacterProfile WithCharacterAppearance(HumanoidCharacterAppearance appearance) =>
        new(this) { Appearance = appearance };
    public HumanoidCharacterProfile WithClothingPreference(ClothingPreference clothing) =>
        new(this) { Clothing = clothing };
    public HumanoidCharacterProfile WithBackpackPreference(BackpackPreference backpack) =>
        new(this) { Backpack = backpack };
    public HumanoidCharacterProfile WithSpawnPriorityPreference(SpawnPriorityPreference spawnPriority) =>
        new(this) { SpawnPriority = spawnPriority };
    public HumanoidCharacterProfile WithJobPriorities(IEnumerable<KeyValuePair<string, JobPriority>> jobPriorities) =>
        new(this) { _jobPriorities = new Dictionary<string, JobPriority>(jobPriorities) };

    public HumanoidCharacterProfile WithJobPriority(string jobId, JobPriority priority)
    {
        var dictionary = new Dictionary<string, JobPriority>(_jobPriorities);
        if (priority == JobPriority.Never)
            dictionary.Remove(jobId);
        else
            dictionary[jobId] = priority;

        return new(this) { _jobPriorities = dictionary };
    }

    public HumanoidCharacterProfile WithPreferenceUnavailable(PreferenceUnavailableMode mode) =>
        new(this) { PreferenceUnavailable = mode };
    public HumanoidCharacterProfile WithAntagPreferences(IEnumerable<string> antagPreferences) =>
        new(this) { _antagPreferences = new HashSet<string>(antagPreferences) };

    public HumanoidCharacterProfile WithAntagPreference(string antagId, bool pref)
    {
        var list = new HashSet<string>(_antagPreferences);
        if (pref)
            list.Add(antagId);
        else
            list.Remove(antagId);

        return new(this) { _antagPreferences = list };
    }

    public HumanoidCharacterProfile WithTraitPreference(string traitId, bool pref)
    {
        var list = new HashSet<string>(_traitPreferences);

        if (pref)
            list.Add(traitId);
        else
            list.Remove(traitId);

        return new(this) { _traitPreferences = list };
    }

    public HumanoidCharacterProfile WithLoadoutPreference(
        string loadoutId,
        bool pref,
        string? customName = null,
        string? customDescription = null,
        string? customColor = null,
        bool? customHeirloom = null)
    {
        var list = new HashSet<LoadoutPreference>(_loadoutPreferences);

        list.RemoveWhere(l => l.LoadoutName == loadoutId);
        if (pref)
            list.Add(new(loadoutId, customName, customDescription, customColor, customHeirloom) { Selected = pref });

        return new HumanoidCharacterProfile(this) { _loadoutPreferences = list };
    }

    public string Summary =>
        Loc.GetString(
            "humanoid-character-profile-summary",
            ("name", Name),
            ("gender", Gender.ToString().ToLowerInvariant()),
            ("age", Age)
        );

    public bool MemberwiseEquals(ICharacterProfile maybeOther)
    {
        return maybeOther is HumanoidCharacterProfile other
            && Name == other.Name
            && Age == other.Age
            && Sex == other.Sex
            && Gender == other.Gender
            && Species == other.Species
            && PreferenceUnavailable == other.PreferenceUnavailable
            && SpawnPriority == other.SpawnPriority
            && _jobPriorities.SequenceEqual(other._jobPriorities)
            && _antagPreferences.SequenceEqual(other._antagPreferences)
            && _traitPreferences.SequenceEqual(other._traitPreferences)
            && LoadoutPreferences.SequenceEqual(other.LoadoutPreferences)
            && Appearance.MemberwiseEquals(other.Appearance)
            && FlavorText == other.FlavorText;
    }

    public void EnsureValid(ICommonSession session, IDependencyCollection collection)
    {
        var configManager = collection.Resolve<IConfigurationManager>();
        var prototypeManager = collection.Resolve<IPrototypeManager>();

        if (!prototypeManager.TryIndex<SpeciesPrototype>(Species, out var speciesPrototype) || speciesPrototype.RoundStart == false)
        {
            Species = SharedHumanoidAppearanceSystem.DefaultSpecies;
            speciesPrototype = prototypeManager.Index<SpeciesPrototype>(Species);
        }

        var sex = Sex switch
        {
            Sex.Male => Sex.Male,
            Sex.Female => Sex.Female,
            Sex.Unsexed => Sex.Unsexed,
            _ => Sex.Male // Invalid enum values.
        };

        // ensure the species can be that sex and their age fits the founds
        if (!speciesPrototype.Sexes.Contains(sex))
        {
            sex = speciesPrototype.Sexes[0];
        }

        var age = Math.Clamp(Age, speciesPrototype.MinAge, speciesPrototype.MaxAge);

        var gender = Gender switch
        {
            Gender.Epicene => Gender.Epicene,
            Gender.Female => Gender.Female,
            Gender.Male => Gender.Male,
            Gender.Neuter => Gender.Neuter,
            _ => Gender.Epicene // Invalid enum values.
        };

        string name;
        if (string.IsNullOrEmpty(Name))
        {
            name = GetName(Species, gender);
        }
        else if (Name.Length > MaxNameLength)
        {
            name = Name[..MaxNameLength];
        }
        else
        {
            name = Name;
        }

        name = name.Trim();

        if (configManager.GetCVar(CCVars.RestrictedNames))
        {
            name = RestrictedNameRegex.Replace(name, string.Empty);
        }

        if (configManager.GetCVar(CCVars.ICNameCase))
        {
            // This regex replaces the first character of the first and last words of the name with their uppercase version
            name = ICNameCaseRegex.Replace(name, m => m.Groups["word"].Value.ToUpper());
        }

        var customspeciename =
            !speciesPrototype.CustomName
            || string.IsNullOrEmpty(Customspeciename)
                ? ""
                : Customspeciename.Length > MaxNameLength
                    ? FormattedMessage.RemoveMarkupPermissive(Customspeciename)[..MaxNameLength]
                    : FormattedMessage.RemoveMarkupPermissive(Customspeciename);

        if (string.IsNullOrEmpty(name))
        {
            name = GetName(Species, gender);
        }

        string flavortext;
        if (FlavorText.Length > MaxDescLength)
        {
            flavortext = FormattedMessage.RemoveMarkupPermissive(FlavorText)[..MaxDescLength];
        }
        else
        {
            flavortext = FormattedMessage.RemoveMarkupPermissive(FlavorText);
        }

        var appearance = HumanoidCharacterAppearance.EnsureValid(Appearance, Species, Sex);

        var prefsUnavailableMode = PreferenceUnavailable switch
        {
            PreferenceUnavailableMode.StayInLobby => PreferenceUnavailableMode.StayInLobby,
            PreferenceUnavailableMode.SpawnAsOverflow => PreferenceUnavailableMode.SpawnAsOverflow,
            _ => PreferenceUnavailableMode.StayInLobby // Invalid enum values.
        };

        var spawnPriority = SpawnPriority switch
        {
            SpawnPriorityPreference.None => SpawnPriorityPreference.None,
            SpawnPriorityPreference.Arrivals => SpawnPriorityPreference.Arrivals,
            SpawnPriorityPreference.Cryosleep => SpawnPriorityPreference.Cryosleep,
            _ => SpawnPriorityPreference.None // Invalid enum values.
        };

        var priorities = new Dictionary<string, JobPriority>(JobPriorities
            .Where(p => prototypeManager.TryIndex<JobPrototype>(p.Key, out var job) && job.SetPreference && p.Value switch
            {
                JobPriority.Never => false, // Drop never since that's assumed default.
                JobPriority.Low => true,
                JobPriority.Medium => true,
                JobPriority.High => true,
                _ => false
            }));

        var antags = AntagPreferences
            .Where(id => prototypeManager.TryIndex<AntagPrototype>(id, out var antag) && antag.SetPreference)
            .Distinct()
            .ToList();

        var traits = TraitPreferences
            .Where(prototypeManager.HasIndex<TraitPrototype>)
            .Distinct()
            .ToList();

        var loadouts = LoadoutPreferences
            .Where(l => prototypeManager.HasIndex<LoadoutPrototype>(l.LoadoutName))
            .Distinct()
            .ToList();

        Name = name;
        Customspeciename = customspeciename;
        FlavorText = flavortext;
        Age = age;
        Sex = sex;
        Gender = gender;
        Appearance = appearance;
        SpawnPriority = spawnPriority;

        _jobPriorities.Clear();

        foreach (var (job, priority) in priorities)
        {
            _jobPriorities.Add(job, priority);
        }

        PreferenceUnavailable = prefsUnavailableMode;

        _antagPreferences.Clear();
        _antagPreferences.UnionWith(antags);

        _traitPreferences.Clear();
        _traitPreferences.UnionWith(traits);

        _loadoutPreferences.Clear();
        _loadoutPreferences.UnionWith(loadouts);
    }

    public ICharacterProfile Validated(ICommonSession session, IDependencyCollection collection)
    {
        var profile = new HumanoidCharacterProfile(this);
        profile.EnsureValid(session, collection);
        return profile;
    }

    // Sorry this is kind of weird and duplicated,
    // Working inside these non entity systems is a bit wack
    public static string GetName(string species, Gender gender)
    {
        var namingSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<NamingSystem>();
        return namingSystem.GetName(species, gender);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is HumanoidCharacterProfile other && MemberwiseEquals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(_jobPriorities);
        hashCode.Add(_antagPreferences);
        hashCode.Add(_traitPreferences);
        hashCode.Add(_loadoutPreferences);
        hashCode.Add(Name);
        hashCode.Add(FlavorText);
        hashCode.Add(Species);
        hashCode.Add(Age);
        hashCode.Add((int) Sex);
        hashCode.Add((int) Gender);
        hashCode.Add(Appearance);
        hashCode.Add((int) SpawnPriority);
        hashCode.Add((int) PreferenceUnavailable);
        hashCode.Add(Customspeciename);
        return hashCode.ToHashCode();
    }

    public HumanoidCharacterProfile Clone()
    {
        return new HumanoidCharacterProfile(this);
    }
}
