using System.Linq;
using System.Text.RegularExpressions;
using Content.Shared.CCVar;
using Content.Shared.Clothing.Loadouts.Prototypes;
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

namespace Content.Shared.Preferences
{
    /// <summary>
    /// Character profile. Looks immutable, but uses non-immutable semantics internally for serialization/code sanity purposes.
    /// </summary>
    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class HumanoidCharacterProfile : ICharacterProfile
    {
        public const int MaxNameLength = 48;
        public const int MaxDescLength = 1024;

        private readonly Dictionary<string, JobPriority> _jobPriorities;
        private readonly List<string> _antagPreferences;
        private readonly List<string> _traitPreferences;
        private readonly List<string> _loadoutPreferences;

        private HumanoidCharacterProfile(
            string name,
            string flavortext,
            string species,
            string customspeciename,
            float height,
            float width,
            int age,
            Sex sex,
            Gender gender,
            HumanoidCharacterAppearance appearance,
            ClothingPreference clothing,
            BackpackPreference backpack,
            SpawnPriorityPreference spawnPriority,
            Dictionary<string, JobPriority> jobPriorities,
            PreferenceUnavailableMode preferenceUnavailable,
            List<string> antagPreferences,
            List<string> traitPreferences,
            List<string> loadoutPreferences)
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
            Appearance = appearance;
            Clothing = clothing;
            Backpack = backpack;
            SpawnPriority = spawnPriority;
            _jobPriorities = jobPriorities;
            PreferenceUnavailable = preferenceUnavailable;
            _antagPreferences = antagPreferences;
            _traitPreferences = traitPreferences;
            _loadoutPreferences = loadoutPreferences;
        }

        /// <summary>Copy constructor but with overridable references (to prevent useless copies)</summary>
        private HumanoidCharacterProfile(
            HumanoidCharacterProfile other,
            Dictionary<string, JobPriority> jobPriorities,
            List<string> antagPreferences,
            List<string> traitPreferences,
            List<string> loadoutPreferences)
            : this(other.Name, other.FlavorText, other.Species, other.Customspeciename, other.Height, other.Width, other.Age, other.Sex, other.Gender, other.Appearance,
                other.Clothing, other.Backpack, other.SpawnPriority, jobPriorities, other.PreferenceUnavailable,
                antagPreferences, traitPreferences, loadoutPreferences)
        {
        }

        /// <summary>Copy constructor</summary>
        private HumanoidCharacterProfile(HumanoidCharacterProfile other)
            : this(other, new Dictionary<string, JobPriority>(other.JobPriorities),
                new List<string>(other.AntagPreferences), new List<string>(other.TraitPreferences),
                new List<string>(other.LoadoutPreferences))
        {
        }

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
            HumanoidCharacterAppearance appearance,
            ClothingPreference clothing,
            BackpackPreference backpack,
            SpawnPriorityPreference spawnPriority,
            IReadOnlyDictionary<string, JobPriority> jobPriorities,
            PreferenceUnavailableMode preferenceUnavailable,
            IReadOnlyList<string> antagPreferences,
            IReadOnlyList<string> traitPreferences,
            IReadOnlyList<string> loadoutPreferences)
            : this(name, flavortext, species, customspeciename, height, width, age, sex, gender, appearance, clothing, backpack, spawnPriority,
                new Dictionary<string, JobPriority>(jobPriorities), preferenceUnavailable,
                new List<string>(antagPreferences), new List<string>(traitPreferences),
                new List<string>(loadoutPreferences))
        {
        }

        /// <summary>
        ///     Get the default humanoid character profile, using internal constant values.
        ///     Defaults to <see cref="SharedHumanoidAppearanceSystem.DefaultSpecies"/> for the species.
        /// </summary>
        /// <returns></returns>
        public HumanoidCharacterProfile() : this(
            "John Doe",
            "",
            SharedHumanoidAppearanceSystem.DefaultSpecies,
            "",
            1f,
            1f,
            18,
            Sex.Male,
            Gender.Male,
            new HumanoidCharacterAppearance(),
            ClothingPreference.Jumpsuit,
            BackpackPreference.Backpack,
            SpawnPriorityPreference.None,
            new Dictionary<string, JobPriority>
            {
                {SharedGameTicker.FallbackOverflowJob, JobPriority.High}
            },
            PreferenceUnavailableMode.SpawnAsOverflow,
            new List<string>(),
            new List<string>(),
            new List<string>())
        {
        }

        /// <summary>
        ///     Return a default character profile, based on species.
        /// </summary>
        /// <param name="species">The species to use in this default profile. The default species is <see cref="SharedHumanoidAppearanceSystem.DefaultSpecies"/>.</param>
        /// <returns>Humanoid character profile with default settings.</returns>
        public static HumanoidCharacterProfile DefaultWithSpecies(string species = SharedHumanoidAppearanceSystem.DefaultSpecies)
        {
            return new(
                "John Doe",
                "",
                species,
                "",
                1f,
                1f,
                18,
                Sex.Male,
                Gender.Male,
                HumanoidCharacterAppearance.DefaultWithSpecies(species),
                ClothingPreference.Jumpsuit,
                BackpackPreference.Backpack,
                SpawnPriorityPreference.None,
                new Dictionary<string, JobPriority>
                {
                    {SharedGameTicker.FallbackOverflowJob, JobPriority.High}
                },
                PreferenceUnavailableMode.SpawnAsOverflow,
                new List<string>(),
                new List<string>(),
                new List<string>());
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
            var height = 1f;
            var width = 1f;
            if (prototypeManager.TryIndex<SpeciesPrototype>(species, out var speciesPrototype))
            {
                sex = random.Pick(speciesPrototype.Sexes);
                age = random.Next(speciesPrototype.MinAge, speciesPrototype.OldAge); // people don't look and keep making 119 year old characters with zero rp, cap it at middle aged
                height = random.NextFloat(speciesPrototype.MinHeight, speciesPrototype.MaxHeight);
                width = random.NextFloat(speciesPrototype.MinWidth, speciesPrototype.MaxWidth);
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

            return new HumanoidCharacterProfile(name, "", species, species, height, width, age, sex, gender,
                HumanoidCharacterAppearance.Random(species, sex), ClothingPreference.Jumpsuit,
                BackpackPreference.Backpack, SpawnPriorityPreference.None,
                new Dictionary<string, JobPriority>
                {
                    {SharedGameTicker.FallbackOverflowJob, JobPriority.High},
                }, PreferenceUnavailableMode.StayInLobby, new List<string>(), new List<string>(), new List<string>());
        }

        public string Name { get; private set; }
        public string FlavorText { get; private set; }

        [DataField("species")]
        public string Species { get; private set; }

        [DataField]
        public string Customspeciename { get; private set; }

        [DataField("height")]
        public float Height { get; private set; }

        [DataField("width")]
        public float Width { get; private set; }

        [DataField("age")]
        public int Age { get; private set; }

        [DataField("sex")]
        public Sex Sex { get; private set; }

        [DataField("gender")]
        public Gender Gender { get; private set; }

        public ICharacterAppearance CharacterAppearance => Appearance;

        [DataField("appearance")]
        public HumanoidCharacterAppearance Appearance { get; private set; }
        public ClothingPreference Clothing { get; private set; }
        public BackpackPreference Backpack { get; private set; }
        public SpawnPriorityPreference SpawnPriority { get; private set; }
        public IReadOnlyDictionary<string, JobPriority> JobPriorities => _jobPriorities;
        public IReadOnlyList<string> AntagPreferences => _antagPreferences;
        public IReadOnlyList<string> TraitPreferences => _traitPreferences;
        public IReadOnlyList<string> LoadoutPreferences => _loadoutPreferences;
        public PreferenceUnavailableMode PreferenceUnavailable { get; private set; }

        public HumanoidCharacterProfile WithName(string name)
        {
            return new(this) { Name = name };
        }

        public HumanoidCharacterProfile WithFlavorText(string flavorText)
        {
            return new(this) { FlavorText = flavorText };
        }

        public HumanoidCharacterProfile WithAge(int age)
        {
            return new(this) { Age = age };
        }

        public HumanoidCharacterProfile WithSex(Sex sex)
        {
            return new(this) { Sex = sex };
        }

        public HumanoidCharacterProfile WithGender(Gender gender)
        {
            return new(this) { Gender = gender };
        }

        public HumanoidCharacterProfile WithSpecies(string species)
        {
            return new(this) { Species = species };
        }

        public HumanoidCharacterProfile WithCustomSpeciesName(string customspeciename)
        {
            return new(this) { Customspeciename = customspeciename };
        }

        public HumanoidCharacterProfile WithHeight(float height)
        {
            return new(this) { Height = height };
        }

        public HumanoidCharacterProfile WithWidth(float width)
        {
            return new(this) { Width = width };
        }

        public HumanoidCharacterProfile WithCharacterAppearance(HumanoidCharacterAppearance appearance)
        {
            return new(this) { Appearance = appearance };
        }

        public HumanoidCharacterProfile WithClothingPreference(ClothingPreference clothing)
        {
            return new(this) { Clothing = clothing };
        }
        public HumanoidCharacterProfile WithBackpackPreference(BackpackPreference backpack)
        {
            return new(this) { Backpack = backpack };
        }
        public HumanoidCharacterProfile WithSpawnPriorityPreference(SpawnPriorityPreference spawnPriority)
        {
            return new(this) { SpawnPriority = spawnPriority };
        }
        public HumanoidCharacterProfile WithJobPriorities(IEnumerable<KeyValuePair<string, JobPriority>> jobPriorities)
        {
            return new(this, new Dictionary<string, JobPriority>(jobPriorities), _antagPreferences, _traitPreferences,
                _loadoutPreferences);
        }

        public HumanoidCharacterProfile WithJobPriority(string jobId, JobPriority priority)
        {
            var dictionary = new Dictionary<string, JobPriority>(_jobPriorities);
            if (priority == JobPriority.Never)
            {
                dictionary.Remove(jobId);
            }
            else
            {
                dictionary[jobId] = priority;
            }
            return new(this, dictionary, _antagPreferences, _traitPreferences, _loadoutPreferences);
        }

        public HumanoidCharacterProfile WithPreferenceUnavailable(PreferenceUnavailableMode mode)
        {
            return new(this) { PreferenceUnavailable = mode };
        }

        public HumanoidCharacterProfile WithAntagPreferences(IEnumerable<string> antagPreferences)
        {
            return new(this, _jobPriorities, new List<string>(antagPreferences), _traitPreferences,
                _loadoutPreferences);
        }

        public HumanoidCharacterProfile WithAntagPreference(string antagId, bool pref)
        {
            var list = new List<string>(_antagPreferences);
            if (pref)
            {
                if (!list.Contains(antagId))
                {
                    list.Add(antagId);
                }
            }
            else
            {
                if (list.Contains(antagId))
                {
                    list.Remove(antagId);
                }
            }
            return new(this, _jobPriorities, list, _traitPreferences, _loadoutPreferences);
        }

        public HumanoidCharacterProfile WithTraitPreference(string traitId, bool pref)
        {
            var list = new List<string>(_traitPreferences);

            // TODO: Maybe just refactor this to HashSet? Same with _antagPreferences
            if (pref)
            {
                if (!list.Contains(traitId))
                {
                    list.Add(traitId);
                }
            }
            else
            {
                if (list.Contains(traitId))
                {
                    list.Remove(traitId);
                }
            }
            return new(this, _jobPriorities, _antagPreferences, list, _loadoutPreferences);
        }

        public HumanoidCharacterProfile WithLoadoutPreference(string loadoutId, bool pref)
        {
            var list = new List<string>(_loadoutPreferences);

            if(pref)
            {
                if(!list.Contains(loadoutId))
                {
                    list.Add(loadoutId);
                }
            }
            else
            {
                if(list.Contains(loadoutId))
                {
                    list.Remove(loadoutId);
                }
            }
            return new(this, _jobPriorities, _antagPreferences, _traitPreferences, list);
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
            if (maybeOther is not HumanoidCharacterProfile other
                || Name != other.Name
                || Age != other.Age
                || Height != other.Height
                || Width != other.Width
                || Sex != other.Sex
                || Gender != other.Gender
                || PreferenceUnavailable != other.PreferenceUnavailable
                || Clothing != other.Clothing
                || Backpack != other.Backpack
                || SpawnPriority != other.SpawnPriority
                || !_jobPriorities.SequenceEqual(other._jobPriorities)
                || !_antagPreferences.SequenceEqual(other._antagPreferences)
                || !_traitPreferences.SequenceEqual(other._traitPreferences)
                || !_loadoutPreferences.SequenceEqual(other._loadoutPreferences))
                return false;
            return Appearance.MemberwiseEquals(other.Appearance);
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
                sex = speciesPrototype.Sexes[0];

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
                name = Regex.Replace(name, @"[^\u0030-\u0039,\u0041-\u005A,\u0061-\u007A,\u00C0-\u00D6,\u00D8-\u00F6,\u00F8-\u00FF,\u0100-\u017F, '.,-]", string.Empty);
                /*
                 * 0030-0039  Basic Latin: ASCII Digits
                 * 0041-005A  Basic Latin: Uppercase Latin Alphabet
                 * 0061-007A  Basic Latin: Lowercase Latin Alphabet
                 * 00C0-00D6  Latin-1 Supplement: Letters I
                 * 00D8-00F6  Latin-1 Supplement: Letters II
                 * 00F8-00FF  Latin-1 Supplement: Letters III
                 * 0100-017F  Latin Extended A: European Latin
                 */
            }

            if (configManager.GetCVar(CCVars.ICNameCase))
            {
                // This regex replaces the first character of the first and last words of the name with their uppercase version
                name = Regex.Replace(name,
                    @"^(?<word>\w)|\b(?<word>\w)(?=\w*$)",
                    m => m.Groups["word"].Value.ToUpper());
            }

            if (string.IsNullOrEmpty(name))
            {
                name = GetName(Species, gender);
            }

            var customspeciename = speciesPrototype.CustomName
                ? FormattedMessage.RemoveMarkup(Customspeciename ?? "")[..MaxNameLength]
                : "";

            string flavortext;
            if (FlavorText.Length > MaxDescLength)
            {
                flavortext = FormattedMessage.RemoveMarkup(FlavorText)[..MaxDescLength];
            }
            else
            {
                flavortext = FormattedMessage.RemoveMarkup(FlavorText);
            }

            var height = Height;
            if (speciesPrototype != null)
                height = Math.Clamp(Height, speciesPrototype.MinHeight, speciesPrototype.MaxHeight);

            var width = Width;
            if (speciesPrototype != null)
                width = Math.Clamp(Width, speciesPrototype.MinWidth, speciesPrototype.MaxWidth);

            var appearance = HumanoidCharacterAppearance.EnsureValid(Appearance, Species, Sex);

            var prefsUnavailableMode = PreferenceUnavailable switch
            {
                PreferenceUnavailableMode.StayInLobby => PreferenceUnavailableMode.StayInLobby,
                PreferenceUnavailableMode.SpawnAsOverflow => PreferenceUnavailableMode.SpawnAsOverflow,
                _ => PreferenceUnavailableMode.StayInLobby // Invalid enum values.
            };

            var clothing = Clothing switch
            {
                ClothingPreference.Jumpsuit => ClothingPreference.Jumpsuit,
                ClothingPreference.Jumpskirt => ClothingPreference.Jumpskirt,
                _ => ClothingPreference.Jumpsuit // Invalid enum values.
            };

            var backpack = Backpack switch
            {
                BackpackPreference.Backpack => BackpackPreference.Backpack,
                BackpackPreference.Satchel => BackpackPreference.Satchel,
                BackpackPreference.Duffelbag => BackpackPreference.Duffelbag,
                _ => BackpackPreference.Backpack // Invalid enum values.
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
                .ToList();

            var traits = TraitPreferences
                .Where(prototypeManager.HasIndex<TraitPrototype>)
                .ToList();

            var maxTraits = configManager.GetCVar(CCVars.GameTraitsMax);
            var currentTraits = 0;
            var traitPoints = configManager.GetCVar(CCVars.GameTraitsDefaultPoints);

            foreach (var trait in traits.OrderBy(t => -prototypeManager.Index<TraitPrototype>(t).Points).ToList())
            {
                var proto = prototypeManager.Index<TraitPrototype>(trait);

                if (traitPoints + proto.Points < 0 || currentTraits + 1 > maxTraits)
                    traits.Remove(trait);
                else
                {
                    traitPoints += proto.Points;
                    currentTraits++;
                }
            }


            var loadouts = LoadoutPreferences
                .Where(prototypeManager.HasIndex<LoadoutPrototype>)
                .ToList();

            var loadoutPoints = configManager.GetCVar(CCVars.GameLoadoutsPoints);
            var currentPoints = 0;

            foreach (var loadout in loadouts.ToList())
            {
                var proto = prototypeManager.Index<LoadoutPrototype>(loadout);

                if (currentPoints + proto.Cost > loadoutPoints)
                    loadouts.Remove(loadout);
                else
                    currentPoints += proto.Cost;
            }


            Name = name;
            Customspeciename = customspeciename;
            FlavorText = flavortext;
            Age = age;
            Height = height;
            Width = width;
            Sex = sex;
            Gender = gender;
            Appearance = appearance;
            Clothing = clothing;
            Backpack = backpack;
            SpawnPriority = spawnPriority;

            _jobPriorities.Clear();

            foreach (var (job, priority) in priorities)
            {
                _jobPriorities.Add(job, priority);
            }

            PreferenceUnavailable = prefsUnavailableMode;

            _antagPreferences.Clear();
            _antagPreferences.AddRange(antags);

            _traitPreferences.Clear();
            _traitPreferences.AddRange(traits);

            _loadoutPreferences.Clear();
            _loadoutPreferences.AddRange(loadouts);
        }

        public ICharacterProfile Validated(ICommonSession session, IDependencyCollection collection)
        {
            var profile = new HumanoidCharacterProfile(this);
            profile.EnsureValid(session, collection);
            return profile;
        }

        // sorry this is kind of weird and duplicated,
        /// working inside these non entity systems is a bit wack
        public static string GetName(string species, Gender gender)
        {
            var namingSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<NamingSystem>();
            return namingSystem.GetName(species, gender);
        }

        public override bool Equals(object? obj)
        {
            return obj is HumanoidCharacterProfile other && MemberwiseEquals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                HashCode.Combine(
                    Name,
                    Species,
                    Age,
                    Sex,
                    Gender,
                    Appearance,
                    Clothing,
                    Backpack
                ),
                HashCode.Combine(
                    SpawnPriority,
                    Height,
                    Width,
                    PreferenceUnavailable,
                    _jobPriorities,
                    _antagPreferences,
                    _traitPreferences,
                    _loadoutPreferences
                ),
                HashCode.Combine(
                    Customspeciename
                )
            );
        }
    }
}
