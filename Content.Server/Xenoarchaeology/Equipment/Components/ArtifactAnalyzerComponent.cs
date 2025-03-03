using Content.Server.Xenoarchaeology.XenoArtifacts;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Xenoarchaeology.Equipment.Components;

/// <summary>
/// A machine that is combined and linked to the <see cref="AnalysisConsoleComponent"/>
/// in order to analyze artifacts and extract points.
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactAnalyzerComponent : Component
{
    /// <summary>
    /// How long it takes to analyze an artifact
    /// </summary>
    [DataField("BaseAnalysisDuration", customTypeSerializer: typeof(TimespanSerializer))]
    public TimeSpan BaseAnalysisDuration = TimeSpan.FromSeconds(40);

    /// <summary>
    /// How long it takes to analyze an artifact with modifiers applied
    /// </summary>
    [DataField("AnalysisDuration", customTypeSerializer: typeof(TimespanSerializer))]
    public TimeSpan AnalysisDuration = TimeSpan.FromSeconds(40);

    /// <summary>
    /// Which machine part affects time reduction
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartTimeReduction = "Manipulator";

    /// <summary>
    /// A multiplier applied to the amount of points generated based on the machine parts inserted.
    /// </summary>
    [DataField]
    public float UpgradeTimeReductionMultiplier = 10;

    // Nyano - Summary - Begin modified code block: tie artifacts to glimmer.
    /// <summary>
    /// Ratio of research points to glimmer.
    /// Each is 150 and added to this, so
    /// 550 / 700 / 850 / 1000
    /// </summary>
    public int ExtractRatio = 400;
    // Nyano - End modified code block.

    /// <summary>
    /// The corresponding console entity.
    /// Can be null if not linked.
    /// </summary>
    [ViewVariables]
    public EntityUid? Console;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool ReadyToPrint = false;

    [DataField("scanFinishedSound")]
    public SoundSpecifier ScanFinishedSound = new SoundPathSpecifier("/Audio/Machines/scan_finish.ogg");

    #region Analysis Data
    [DataField]
    public EntityUid? LastAnalyzedArtifact;

    [ViewVariables]
    public ArtifactNode? LastAnalyzedNode;

    [ViewVariables(VVAccess.ReadWrite)]
    public int? LastAnalyzerPointValue;
    #endregion
}
