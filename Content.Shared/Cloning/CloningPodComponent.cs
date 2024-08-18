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
    ///     How long the cloning has been going on for.
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
    ///     The material that is used to clone entities.
    /// </summary>
    [DataField]
    public ProtoId<MaterialPrototype> RequiredMaterial = "Biomass";

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
    ///     Emag sound effects.
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

    [ViewVariables(VVAccess.ReadWrite)]
    public CloningPodStatus Status;

    [ViewVariables]
    public EntityUid? ConnectedConsole;

    /// <summary>
    ///     Tracks whether a Cloner is actively cloning someone.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ActivelyCloning;

    #region Metempsychosis

    /// <summary>
    ///     Controls whether a cloning machine performs the Metempsychosis functions, EG: Is this a Cloner or a Metem Machine?
    ///     Metempsychosis refers to the metaphysical process of Reincarnation
    /// </summary>
    [DataField]
    public bool DoMetempsychosis;

    /// <summary>
    ///     How much should each point of Karma decrease the odds of reincarnating as a humanoid.
    /// </summary>
    [DataField]
    public float KarmaOffset = 0.5f;

    /// <summary>
    ///     The base chances for a Metem Machine to produce a Humanoid.
    ///     > 1 has a chance of acting like a true Cloner
    ///     On a successful roll, produces a random Humanoid.
    ///     A failed roll poduces a random NonHumanoid
    /// </summary>
    [DataField]
    public float HumanoidBaseChance = 1;

    /// <summary>
    ///     The proto that the Metem Machine picks a random Humanoid from.
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

/// <summary>
///     Raised after a new mob got spawned when cloning a humanoid
/// </summary>
[ByRefEvent]
public struct CloningEvent
{
    public bool NameHandled = false;

    public readonly EntityUid Source;
    public readonly EntityUid Target;

    public CloningEvent(EntityUid source, EntityUid target)
    {
        Source = source;
        Target = target;
    }
}

/// <summary>
///     Raised on a corpse being subjected to forced reincarnation(Metempsychosis). Allowing for innate effects from the mob to influence the reincarnation.
/// </summary>
[ByRefEvent]
public struct ReincarnatingEvent
{
    public bool OverrideChance;
    public bool NeverTrulyClone;
    public ForcedMetempsychosisType ForcedType = ForcedMetempsychosisType.None;
    public readonly EntityUid OldBody;
    public float ReincarnationChanceModifier = 1;
    public float ReincarnationChances;

    public ReincarnatingEvent(EntityUid oldBody, float reincarnationChances)
    {
        OldBody = oldBody;
        ReincarnationChances = reincarnationChances;
    }
}
