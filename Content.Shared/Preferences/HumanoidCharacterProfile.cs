using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared._White.Bark;
using Content.Shared._White.Bark.Systems;
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

/// <summary>
/// Character profile. Looks immutable, but uses non-immutable semantics internally for serialization/code sanity purposes.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class HumanoidCharacterProfile : ICharacterProfile
{
    private static readonly Regex RestrictedNameRegex = new(@"[^A-Za-z0-9А-Яа-я '\-]");
    private static readonly Regex ICNameCaseRegex = new(@"^(?<word>\w)|\b(?<word>\w)(?=\w*$)");

    public const int MaxNameLength = 64;
    public const int MaxDescLength = 1024;
    public const int MaxCustomContentLength = 524288; // WD EDIT

    /// Job preferences for initial spawn
    [DataField]
    private Dictionary<ProtoId<JobPrototype>, JobPriority> _jobPriorities = new()
    {
        {
            SharedGameTicker.FallbackOverflowJob, JobPriority.High
        },
    };

    /// Antags we have opted in to
    [DataField]
    private HashSet<ProtoId<AntagPrototype>> _antagPreferences = new();

    /// Enabled traits
    [DataField]
    private HashSet<ProtoId<TraitPrototype>> _traitPreferences = new();

    /// <see cref="_loadoutPreferences"/>
    public Dictionary<string, Loadout> LoadoutPreferences => _loadoutPreferences; // WWDP EDIT
    public IEnumerable<Loadout> LoadoutPreferencesList => _loadoutPreferences.Values; // WWDP EDIT

    [DataField]
    private Dictionary<string, Loadout> _loadoutPreferences = new(); // WWDP EDIT

    [DataField]
    public string Name { get; set; } = "John Doe";

    /// Detailed text that can appear for the character if <see cref="CCVars.FlavorText"/> is enabled
    [DataField]
    public string FlavorText { get; set; } = string.Empty;

    /// Associated <see cref="SpeciesPrototype"/> for this profile
    [DataField]
    public ProtoId<SpeciesPrototype> Species { get; set; } = SharedHumanoidAppearanceSystem.DefaultSpecies;

    // EE -- Contractors Change Start
    [DataField]
    public string Nationality { get; set; } = SharedHumanoidAppearanceSystem.DefaultNationality;

    [DataField]
    public string Employer { get; set; } = SharedHumanoidAppearanceSystem.DefaultEmployer;

    [DataField]
    public string Lifepath { get; set; } = SharedHumanoidAppearanceSystem.DefaultLifepath;
    // EE -- Contractors Change End

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

    // WD EDIT START
    [DataField]
    public string BodyType { get; set; } = SharedHumanoidAppearanceSystem.DefaultBodyType;

    [DataField]
    public string BarkVoice { get; set; } = SharedHumanoidAppearanceSystem.DefaultBarkVoice;

    [DataField]
    public BarkPercentageApplyData BarkSettings { get; set; } = BarkPercentageApplyData.Default;
    // WD EDIT END

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

    /// When spawning into a round what's the preferred spot to spawn
    [DataField]
    public SpawnPriorityPreference SpawnPriority { get; private set; } = SpawnPriorityPreference.None;

    /// <see cref="_jobPriorities"/>
    public IReadOnlyDictionary<ProtoId<JobPrototype>, JobPriority> JobPriorities => _jobPriorities;

    /// <see cref="_antagPreferences"/>
    public IReadOnlySet<ProtoId<AntagPrototype>> AntagPreferences => _antagPreferences;

    /// <see cref="_traitPreferences"/>
    public IReadOnlySet<ProtoId<TraitPrototype>> TraitPreferences => _traitPreferences;

    /// If we're unable to get one of our preferred jobs do we spawn as a fallback job or do we stay in lobby
    [DataField]
    public PreferenceUnavailableMode PreferenceUnavailable { get; private set; } =
        PreferenceUnavailableMode.SpawnAsOverflow;

    public HumanoidCharacterProfile(
        string name,
        string flavortext,
        string species,
        string customspeciename,
        // EE -- Contractors Change Start
        string nationality,
        string employer,
        string lifepath,
        // EE -- Contractors Change End
        float height,
        float width,
        int age,
        Sex sex,
        string barkVoice, // WD EDIT
        BarkPercentageApplyData barkSettings, // WD EDIT
        string bodyType, // WD EDIT
        Gender gender,
        string? displayPronouns,
        string? stationAiName,
        string? cyborgName,
        HumanoidCharacterAppearance appearance,
        SpawnPriorityPreference spawnPriority,
        Dictionary<ProtoId<JobPrototype>, JobPriority> jobPriorities,
        PreferenceUnavailableMode preferenceUnavailable,
        HashSet<ProtoId<AntagPrototype>> antagPreferences,
        HashSet<ProtoId<TraitPrototype>> traitPreferences,
        Dictionary<string, Loadout> loadoutPreferences) // WWDP EDIT
    {
        Name = name;
        FlavorText = flavortext;
        Species = species;
        Customspeciename = customspeciename;
        // EE -- Contractors Change Start
        Nationality = nationality;
        Employer = employer;
        Lifepath = lifepath;
        // EE -- Contractors Change End
        Height = height;
        Width = width;
        Age = age;
        Sex = sex;
        BarkVoice = barkVoice; // WD EDIT
        BodyType = bodyType; // WD EDIT
        BarkSettings = barkSettings.Clone(); // WD EDIT
        Gender = gender;
        DisplayPronouns = displayPronouns;
        StationAiName = stationAiName;
        CyborgName = cyborgName;
        Appearance = appearance;
        SpawnPriority = spawnPriority;
        _jobPriorities = jobPriorities;
        PreferenceUnavailable = preferenceUnavailable;
        _antagPreferences = antagPreferences;
        _traitPreferences = traitPreferences;
        _loadoutPreferences = loadoutPreferences;

        var hasHighPrority = false;
        foreach (var (key, value) in _jobPriorities)
        {
            if (value == JobPriority.Never)
                _jobPriorities.Remove(key);
            else if (value != JobPriority.High)
                continue;

            if (hasHighPrority)
                _jobPriorities[key] = JobPriority.Medium;

            hasHighPrority = true;
        }
    }

    /// <summary>Copy constructor</summary>
    public HumanoidCharacterProfile(HumanoidCharacterProfile other)
        : this(
            other.Name,
            other.FlavorText,
            other.Species,
            other.Customspeciename,
            // EE -- Contractors Change Start
            other.Nationality,
            other.Employer,
            other.Lifepath,
            // EE -- Contractors Change End
            other.Height,
            other.Width,
            other.Age,
            other.Sex,
            other.BarkVoice, // WD EDIT
            other.BarkSettings.Clone(), // WD EDIT
            other.BodyType, // WD EDIT
            other.Gender,
            other.DisplayPronouns,
            other.StationAiName,
            other.CyborgName,
            other.Appearance.Clone(),
            other.SpawnPriority,
            new Dictionary<ProtoId<JobPrototype>, JobPriority>(other.JobPriorities),
            other.PreferenceUnavailable,
            new HashSet<ProtoId<AntagPrototype>>(other.AntagPreferences),
            new HashSet<ProtoId<TraitPrototype>>(other.TraitPreferences),
            new Dictionary<string, Loadout>(other.LoadoutPreferences)) // WWDP EDIT
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
            Nationality = SharedHumanoidAppearanceSystem.DefaultNationality,
            Employer = SharedHumanoidAppearanceSystem.DefaultEmployer,
            Lifepath = SharedHumanoidAppearanceSystem.DefaultLifepath,
        };
    }

    // TODO: This should eventually not be a visual change only.
    public static HumanoidCharacterProfile Random(HashSet<string>? ignoredSpecies = null)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var random = IoCManager.Resolve<IRobustRandom>();
        // WWDP edit start
        var specieslist = prototypeManager
            .EnumeratePrototypes<SpeciesPrototype>()
            .Where(x => !ignoredSpecies?.Contains(x.ID) ?? true) // WWDP
            .ToArray();

        if (specieslist.Length == 0) // Fallback
            specieslist = [prototypeManager.Index<SpeciesPrototype>(SharedHumanoidAppearanceSystem.DefaultSpecies)];

        var species = random.Pick(specieslist).ID;
        // WWDP edit end

        return RandomWithSpecies(species);
    }

    public static HumanoidCharacterProfile RandomWithSpecies(string species = SharedHumanoidAppearanceSystem.DefaultSpecies)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var random = IoCManager.Resolve<IRobustRandom>();

        var sex = Sex.Unsexed;
        var age = 18;
        var bodyType = SharedHumanoidAppearanceSystem.DefaultBodyType; // WD EDIT
        if (prototypeManager.TryIndex<SpeciesPrototype>(species, out var speciesPrototype))
        {
            sex = random.Pick(speciesPrototype.Sexes);
            age = random.Next(speciesPrototype.MinAge, speciesPrototype.OldAge); // people don't look and keep making 119 year old characters with zero rp, cap it at middle aged
            bodyType = speciesPrototype.BodyTypes.First(); // WD EDIT
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

        var profile = new HumanoidCharacterProfile() // WD EDIT
        {
            Name = name,
            Sex = sex,
            Age = age,
            Gender = gender,
            BodyType = bodyType, // WD EDIT
            Species = species,
            Appearance = HumanoidCharacterAppearance.Random(species, sex),
            Nationality = SharedHumanoidAppearanceSystem.DefaultNationality,
            Employer = SharedHumanoidAppearanceSystem.DefaultEmployer,
            Lifepath = SharedHumanoidAppearanceSystem.DefaultLifepath,
        };

        // WD EDIT START
        var barkSystem = IoCManager.Resolve<IEntityManager>().System<SharedBarkSystem>();
        var barkVoiceList = barkSystem.GetVoiceList(profile);

        var barkVoice = SharedHumanoidAppearanceSystem.DefaultBarkVoice;
        if (barkVoiceList.Any())
        {
            barkVoice = random.Pick(barkVoiceList).ID;
        }

        profile.BarkVoice = barkVoice;

        return profile;
        // WD EDIT END
    }

    public HumanoidCharacterProfile WithName(string name) => new(this) { Name = name };
    public HumanoidCharacterProfile WithFlavorText(string flavorText) => new(this) { FlavorText = flavorText };
    public HumanoidCharacterProfile WithBodyType(string bodyType) => new(this) { BodyType = bodyType }; // WD EDIT
    public HumanoidCharacterProfile WithBarkVoice(string barkVoice, BarkPercentageApplyData setting) =>
        new(this) { BarkVoice = barkVoice, BarkSettings = setting.Clone() }; // WD EDIT

    public HumanoidCharacterProfile WithAge(int age) => new(this) { Age = age };
    // EE - Contractors Change Start
    public HumanoidCharacterProfile WithNationality(string nationality) => new(this) { Nationality = nationality };
    public HumanoidCharacterProfile WithEmployer(string employer) => new(this) { Employer = employer };
    public HumanoidCharacterProfile WithLifepath(string lifepath) => new(this) { Lifepath = lifepath };
    // EE - Contractors Change End
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

    public HumanoidCharacterProfile WithSpawnPriorityPreference(SpawnPriorityPreference spawnPriority) =>
        new(this) { SpawnPriority = spawnPriority };

    public HumanoidCharacterProfile WithJobPriorities(IEnumerable<KeyValuePair<ProtoId<JobPrototype>, JobPriority>> jobPriorities)
    {
        var dictionary = new Dictionary<ProtoId<JobPrototype>, JobPriority>(jobPriorities);
        var hasHighPrority = false;

        foreach (var (key, value) in dictionary)
        {
            if (value == JobPriority.Never)
                dictionary.Remove(key);
            else if (value != JobPriority.High)
                continue;

            if (hasHighPrority)
                dictionary[key] = JobPriority.Medium;

            hasHighPrority = true;
        }

        return new(this)
        {
            _jobPriorities = dictionary
        };
    }

    public HumanoidCharacterProfile WithJobPriority(ProtoId<JobPrototype> jobId, JobPriority priority)
    {
        var dictionary = new Dictionary<ProtoId<JobPrototype>, JobPriority>(_jobPriorities);
        if (priority == JobPriority.Never)
            dictionary.Remove(jobId);
        else if (priority == JobPriority.High)
        {
            // There can only ever be one high priority job.
            foreach (var (job, value) in dictionary)
            {
                if (value == JobPriority.High)
                    dictionary[job] = JobPriority.Medium;
            }

            dictionary[jobId] = priority;
        }
        else
            dictionary[jobId] = priority;

        return new(this) { _jobPriorities = dictionary };
    }

    public HumanoidCharacterProfile WithPreferenceUnavailable(PreferenceUnavailableMode mode) =>
        new(this) { PreferenceUnavailable = mode };
    public HumanoidCharacterProfile WithAntagPreferences(IEnumerable<ProtoId<AntagPrototype>> antagPreferences) =>
        new(this) { _antagPreferences = new HashSet<ProtoId<AntagPrototype>>(antagPreferences) };

    public HumanoidCharacterProfile WithAntagPreference(ProtoId<AntagPrototype> antagId, bool pref)
    {
        var list = new HashSet<ProtoId<AntagPrototype>>(_antagPreferences);
        if (pref)
            list.Add(antagId);
        else
            list.Remove(antagId);

        return new(this) { _antagPreferences = list };
    }

    public HumanoidCharacterProfile WithTraitPreference(ProtoId<TraitPrototype> traitId, bool pref)
    {
        var list = new HashSet<ProtoId<TraitPrototype>>(_traitPreferences);

        if (pref)
            list.Add(traitId);
        else
            list.Remove(traitId);

        return new(this) { _traitPreferences = list };
    }

    // WWDP EDIT START
    // I'll rip the hands off whoever coded this piece of shit named Loadouts
    public HumanoidCharacterProfile WithLoadoutPreference(List<Loadout> loadouts)
    {
        var dictionary = loadouts.ToDictionary(p => p.LoadoutName);
        return new(this) { _loadoutPreferences = dictionary };
    }
    // WWDP EDIT END

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
            && BarkVoice == other.BarkVoice // WD EDIT
            && BodyType == other.BodyType // WD EDIT
            && Gender == other.Gender
            && Species == other.Species
            // EE - Contractors Change Start
            && Nationality == other.Nationality
            && Employer == other.Employer
            && Lifepath == other.Lifepath
            // EE - Contractors Change End
            && PreferenceUnavailable == other.PreferenceUnavailable
            && SpawnPriority == other.SpawnPriority
            && _jobPriorities.SequenceEqual(other._jobPriorities)
            && _antagPreferences.SequenceEqual(other._antagPreferences)
            && _traitPreferences.SequenceEqual(other._traitPreferences)
            && LoadoutPreferences.SequenceEqual(other.LoadoutPreferences)
            && Appearance.MemberwiseEquals(other.Appearance)
            && FlavorText == other.FlavorText;
    }

    public void EnsureValid(ICommonSession session, IDependencyCollection collection, string[] sponsorPrototypes)
    {
        var configManager = collection.Resolve<IConfigurationManager>();
        var prototypeManager = collection.Resolve<IPrototypeManager>();

        if (!prototypeManager.TryIndex(Species, out var speciesPrototype) || speciesPrototype.RoundStart == false)
        {
            Species = SharedHumanoidAppearanceSystem.DefaultSpecies;
            speciesPrototype = prototypeManager.Index(Species);
        }

        // Corvax-Sponsors-Start: Reset to human if player not sponsor
        if (speciesPrototype.SponsorOnly && !sponsorPrototypes.Contains(Species.Id))
        {
            Species = SharedHumanoidAppearanceSystem.DefaultSpecies;
            speciesPrototype = prototypeManager.Index(Species);
        }
        // Corvax-Sponsors-End

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

        var bodyType = speciesPrototype.BodyTypes.Contains(BodyType) ? BodyType : speciesPrototype.BodyTypes.First(); // WD EDIT

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

        var appearance = HumanoidCharacterAppearance.EnsureValid(Appearance, Species, Sex, sponsorPrototypes);

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

        var priorities = new Dictionary<ProtoId<JobPrototype>, JobPriority>(JobPriorities
            .Where(p => prototypeManager.TryIndex<JobPrototype>(p.Key, out var job) && job.SetPreference && p.Value switch
            {
                JobPriority.Never => false, // Drop never since that's assumed default.
                JobPriority.Low => true,
                JobPriority.Medium => true,
                JobPriority.High => true,
                _ => false
            }));

        var hasHighPrio = false;
        foreach (var (key, value) in priorities)
        {
            if (value != JobPriority.High)
                continue;

            if (hasHighPrio)
                priorities[key] = JobPriority.Medium;
            hasHighPrio = true;
        }

        var antags = AntagPreferences
            .Where(id => prototypeManager.TryIndex(id, out var antag) && antag.SetPreference)
            .Distinct()
            .ToList();

        var traits = TraitPreferences
            .Where(x => prototypeManager.TryIndex(x, out var trait)
                && (!trait.SponsorOnly || sponsorPrototypes.Contains(x.Id)))
            .Distinct()
            .ToList();

        var loadouts = LoadoutPreferences
            .Where(l => prototypeManager.HasIndex<LoadoutPrototype>(l.Key))
            .ToList();

        Name = name;
        Customspeciename = customspeciename;
        FlavorText = flavortext;
        Age = age;
        Sex = sex;
        Gender = gender;
        BodyType = bodyType; // WD EDIT
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

        // WD EDIT START
        if(!CanHaveBark(prototypeManager, collection))
            BarkVoice = SharedHumanoidAppearanceSystem.DefaultBarkVoice;

        foreach (var (key, loadout) in loadouts)
        {
            if (loadout.CustomContent is not { Length: > MaxCustomContentLength, })
            {
                _loadoutPreferences[key] = loadout;
                continue;
            }
            var truncated = loadout.CustomContent.AsSpan(0, MaxCustomContentLength);
            while (truncated.Length > 0 && char.IsLowSurrogate(truncated[^1]))
                truncated = truncated[..^1];

            var truncatedLoadout = new Loadout(
                    loadout.LoadoutName,
                    loadout.CustomName,
                    loadout.CustomDescription,
                    truncated.ToString(),
                    loadout.CustomColorTint,
                    loadout.CustomHeirloom);

            _loadoutPreferences[key] = truncatedLoadout;
        }
        // WD EDIT END
    }

    // WD EDIT START
    public bool CanHaveBark(
        IPrototypeManager prototypeManager,IDependencyCollection collection,
        ProtoId<BarkListPrototype>? id = null
    )
    {
        var voice = BarkVoice;
        if(
            !prototypeManager.TryIndex<BarkListPrototype>(id ?? "default", out var barkList) ||
            !barkList.VoiceList.TryGetValue(voice, out var voiceRequirements) ||
            !prototypeManager.TryIndex<BarkVoicePrototype>(voice, out var voicePrototype))
        {
            return false;
        }

        var isValid = true;
        var reason = "";

        foreach (var requirement in voiceRequirements)
        {
            var passes = requirement.IsValid(
                default!,
                this,
                new Dictionary<string, TimeSpan>(),
                false,
                voicePrototype,
                collection.Resolve<IEntityManager>(),
                prototypeManager,
                collection.Resolve<IConfigurationManager>(),
                out reason);

            if (passes == !requirement.Inverted)
                continue;

            isValid = false;
            break;
        }

        return isValid;
    }
    // WD EDIT END

    public ICharacterProfile Validated(ICommonSession session, IDependencyCollection collection, string[] sponsorPrototypes)
    {
        var profile = new HumanoidCharacterProfile(this);
        profile.EnsureValid(session, collection, sponsorPrototypes);
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
        hashCode.Add(Employer);
        hashCode.Add(Nationality);
        hashCode.Add(Lifepath);
        hashCode.Add(Age);
        hashCode.Add((int) Sex);
        hashCode.Add((int) Gender);
        hashCode.Add(BodyType); // WD EDIT
        hashCode.Add(BarkVoice); // WD EDIT
        hashCode.Add(BarkSettings); // WD EDIT
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
