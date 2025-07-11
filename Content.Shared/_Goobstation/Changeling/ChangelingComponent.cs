using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChangelingComponent : Component
{
    #region Prototypes

    [DataField("soundMeatPool")]
    public List<SoundSpecifier?> SoundPool = new()
    {
        new SoundPathSpecifier("/Audio/Effects/gib1.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib2.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib3.ogg"),
    };

    [DataField("soundShriek")]
    public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/changeling_shriek.ogg");

    [DataField]
    public float ShriekPower = 2.5f;

    public readonly List<ProtoId<EntityPrototype>> BaseChangelingActions = new()
    {
        "ActionEvolutionMenu",
        "ActionAbsorbDNA",
        "ActionStingExtractDNA",
        "ActionChangelingTransformCycle",
        "ActionChangelingTransform",
        "ActionEnterStasis",
        "ActionExitStasis"
    };

    /// <summary>
    ///     The status icon corresponding to the Changlings.
    /// </summary>

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "HivemindFaction";

    #endregion

    public bool IsInStasis;

    public bool StrainedMusclesActive;

    public bool IsInLesserForm;

    public bool IsInLastResort;

    public List<EntityUid>? ActiveArmor = null;

    public Dictionary<string, EntityUid?> Equipment = new();

    /// <summary>
    ///     Amount of biomass changeling currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Biomass = 60f;

    /// <summary>
    ///     Maximum amount of biomass a changeling can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxBiomass = 30f;

    /// <summary>
    ///     How much biomass should be removed per cycle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BiomassDrain = 1f;

    /// <summary>
    ///     Current amount of chemicals changeling currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Chemicals = 100f;

    /// <summary>
    ///     Maximum amount of chemicals changeling can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxChemicals = 100f;

    /// <summary>
    ///     Bonus chemicals regeneration. In case
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BonusChemicalRegen = 0f;

    /// <summary>
    ///     Cooldown between chem regen events.
    /// </summary>
    public TimeSpan UpdateTimer = TimeSpan.Zero;
    public float UpdateCooldown = 1f;

    public float BiomassUpdateTimer;
    public float BiomassUpdateCooldown = 60f;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<TransformData> AbsorbedDNA = new();
    /// <summary>
    ///     Index of <see cref="AbsorbedDNA"/>. Used for switching forms.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int AbsorbedDNAIndex;

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    public int MaxAbsorbedDNA = 5;

    /// <summary>
    ///     Total absorbed DNA. Counts towards objectives.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int TotalAbsorbedEntities;

    /// <summary>
    ///     Total stolen DNA. Counts towards objectives.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int TotalStolenDNA;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? CurrentForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? SelectedForm;

    [DataField]
    public string AbsorbFailIncapacitated = "changeling-absorb-fail-incapacitated";

    [DataField]
    public string AbsorbFailAbsorbed = "changeling-absorb-fail-absorbed";

    [DataField]
    public string AbsorbFailUnabsorbable = "changeling-absorb-fail-unabsorbable";

    [DataField]
    public string AbsorbFailNoGrab = "changeling-absorb-fail-nograb";

    [DataField]
    public string AbsorbPopup = "changeling-absorb-start";

    [DataField]
    public PopupType AbsorbPopupType = PopupType.LargeCaution;

    [DataField]
    public TimeSpan AbsorbTime = TimeSpan.FromSeconds(15);

    [ValidatePrototypeId<DamageTypePrototype>]
    public string AbsorbedDamageType = "Cellular";

    /// <summary>
    ///     What reagent will the changeling replace their SUCC'ed victims blood with.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> AbsorbedBloodReagent = "FerrochromicAcid";

    /// <summary>
    ///     The total running count of however many evolution points a 'Ling has obtained throughout the round. Including spent points.
    ///     This is used for when a changeling eats another of their kind.
    /// </summary>
    [DataField]
    public int TotalEvolutionPoints;
}

[DataDefinition]
public sealed partial class TransformData
{
    /// <summary>
    ///     Entity's name.
    /// </summary>
    [DataField]
    public string Name;

    /// <summary>
    ///     Entity's fingerprint, if it exists.
    /// </summary>
    [DataField]
    public string? Fingerprint;

    /// <summary>
    ///     Entity's DNA.
    /// </summary>
    [DataField("dna")]
    public string DNA;

    /// <summary>
    ///     Entity's humanoid appearance component.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), NonSerialized]
    public HumanoidAppearanceComponent Appearance;
}
