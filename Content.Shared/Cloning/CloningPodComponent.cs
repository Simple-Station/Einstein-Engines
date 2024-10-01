using Content.Shared.Construction.Prototypes;
using Content.Shared.DeviceLinking;
using Content.Shared.Materials;
using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cloning;

[RegisterComponent]
public sealed partial class CloningPodComponent : Component
{
    [ValidatePrototypeId<SinkPortPrototype>]
    public const string PodPort = "CloningPodReceiver";

    [ViewVariables]
    public ContainerSlot BodyContainer = default!;

    /// <summary>
    ///     How long the cloning has been going on for
    /// </summary>
    [ViewVariables]
    public float CloningProgress = 0;

    [DataField]
    public float BiomassCostMultiplier = 1;

    [ViewVariables]
    public int UsedBiomass = 70;

    [ViewVariables]
    public bool FailedClone = false;

    /// <summary>
    ///     The material that is used to clone entities
    /// </summary>
    [DataField]
    public ProtoId<MaterialPrototype> RequiredMaterial = "Biomass";

    /// <summary>
    ///     The multiplier for cloning duration
    /// </summary>
    [DataField]
    public float PartRatingSpeedMultiplier = 0.75f;

    /// <summary>
    ///     The machine part that affects cloning speed
    /// </summary>
    [DataField]
    public ProtoId<MachinePartPrototype> MachinePartCloningSpeed = "Manipulator";

    /// <summary>
    ///     The current amount of time it takes to clone a body
    /// </summary>
    [DataField]
    public float CloningTime = 30f;

    /// <summary>
    ///     The mob to spawn on emag
    /// </summary>
    [DataField]
    public EntProtoId MobSpawnId = "MobAbomination";

    /// <summary>
    ///     Emag sound effects
    /// </summary>
    [DataField]
    public SoundSpecifier SparkSound = new SoundCollectionSpecifier("sparks")
    {
        Params = AudioParams.Default.WithVolume(8),
    };

    // TODO: Remove this from here when cloning and/or zombies are refactored
    [DataField]
    public SoundSpecifier ScreamSound = new SoundCollectionSpecifier("ZombieScreams")
    {
        Params = AudioParams.Default.WithVolume(4),
    };

    /// <summary>
    ///     The machine part that affects how much biomass is needed to clone a body.
    /// </summary>
    [DataField]
    public float PartRatingMaterialMultiplier = 0.85f;

    /// <summary>
    ///     The machine part that decreases the amount of material needed for cloning
    /// </summary>
    [DataField]
    public ProtoId<MachinePartPrototype> MachinePartMaterialUse = "MatterBin";

    [ViewVariables(VVAccess.ReadWrite)]
    public CloningPodStatus Status;

    [ViewVariables]
    public EntityUid? ConnectedConsole;

    /// <summary>
    ///     Tracks whether a Cloner is actively cloning someone
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ActivelyCloning;

    /// <summary>
    ///     Controls whether a Cloning Pod will add genetic damage to a clone, scaling as the body's crit threshold + 1 + the genetic damage of the body to be cloned
    /// </summary>
    [DataField]
    public bool DoGeneticDamage = true;

    /// <summary>
    ///     How much should the cloning pod adjust the hunger of an entity by
    /// </summary>
    [DataField]
    public float HungerAdjustment = 50;

    /// <summary>
    ///     How much should the cloning pod adjust the thirst of an entity by
    /// </summary>
    [DataField]
    public float ThirstAdjustment = 50;

    /// <summary>
    ///     How much time should the cloning pod give an entity the durnk condition, in seconds
    /// </summary>
    [DataField]
    public float DrunkTimer = 300;

    #region Metempsychosis

    /// <summary>
    ///     Controls whether a cloning machine performs the Metempsychosis functions, EG: Is this a Cloner or a Metem Machine?
    ///     Metempsychosis refers to the metaphysical process of Reincarnation.
    /// </summary>
    /// <remarks>
    ///     A Machine with this enabled will essentially create a random new character instead of creating a living version of the old character.
    ///     Although, the specifics of how much of the new body is a "new character" is highly adjustable in server configuration.
    /// </remarks>
    [DataField]
    public bool DoMetempsychosis;

    /// <summary>
    ///     How much should each point of Karma decrease the odds of reincarnating as a humanoid
    /// </summary>
    [DataField]
    public float KarmaOffset = 0.5f;

    /// <summary>
    ///     The base chances for a Metem Machine to produce a Humanoid.
    ///     > 1 has a chance of acting like a true Cloner.
    ///     On a successful roll, produces a random Humanoid.
    ///     A failed roll poduces a random NonHumanoid.
    /// </summary>
    [DataField]
    public float HumanoidBaseChance = 1;

    /// <summary>
    ///     The proto that the Metem Machine picks a random Humanoid from
    /// </summary>
    [ValidatePrototypeId<WeightedRandomPrototype>]
    [DataField]
    public string MetempsychoticHumanoidPool = "MetempsychoticHumanoidPool";

    /// <summary>
    ///     The proto that the Metem Machine picks a random Non-Humanoid from
    /// </summary>
    [ValidatePrototypeId<WeightedRandomPrototype>]
    [DataField]
    public string MetempsychoticNonHumanoidPool = "MetempsychoticNonhumanoidPool";

    #endregion
}

[Serializable, NetSerializable]
public enum CloningPodVisuals : byte
{
    Status
}

[Serializable, NetSerializable]
public enum CloningPodStatus : byte
{
    Idle,
    Cloning,
    Gore,
    NoMind
}

[Serializable, NetSerializable]
public enum ForcedMetempsychosisType : byte
{
    None,
    Clone,
    RandomHumanoid,
    RandomNonHumanoid
}
