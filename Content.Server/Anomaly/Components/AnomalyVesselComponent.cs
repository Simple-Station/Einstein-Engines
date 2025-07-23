using Content.Shared.Anomaly;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;


namespace Content.Server.Anomaly.Components;

/// <summary>
/// Anomaly Vessels can have an anomaly "stored" in them
/// by interacting on them with an anomaly scanner. Then,
/// they generate points for the selected server based on
/// the anomaly's stability and severity.
/// </summary>
[RegisterComponent, Access(typeof(SharedAnomalySystem)), AutoGenerateComponentPause]
public sealed partial class AnomalyVesselComponent : Component
{
    /// <summary>
    /// The anomaly that the vessel is storing.
    /// Can be null.
    /// </summary>
    [ViewVariables]
    public EntityUid? Anomaly;

    /// <summary>
    /// The base multiplier without any frills
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BasePointMultiplier = 1;

    /// <summary>
    /// The base radiation for only the experimental vessel
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BaseRadiation = .75f;

    /// <summary>
    /// A multiplier applied to the amount of points generated.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float PointMultiplier = 1;

    /// <summary>
    /// A multiplier applied to the amount of points generated based on the machine parts inserted.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float UpgradePointMultiplier = .5f;

    /// <summary>
    /// A multipler applied to the radiation
    /// </summary>
    /// <remarks>
    /// no free ultra point machine 100% legit
    /// </remarks>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float UpgradeRadiationMultiplier = .35f;

    /// <summary>
    ///     Which machine part affects the point multiplier
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartPointMultiplier = "Capacitor";

    /// <summary>
    /// The maximum time between each beep
    /// </summary>
    [DataField("maxBeepInterval")]
    public TimeSpan MaxBeepInterval = TimeSpan.FromSeconds(2f);

    /// <summary>
    /// The minimum time between each beep
    /// </summary>
    [DataField("minBeepInterval")]
    public TimeSpan MinBeepInterval = TimeSpan.FromSeconds(0.75f);

    /// <summary>
    /// When the next beep sound will play
    /// </summary>
    [DataField("nextBeep", customTypeSerializer:typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextBeep = TimeSpan.Zero;

    /// <summary>
    /// The sound that is played repeatedly when the anomaly is destabilizing/decaying
    /// </summary>
    [DataField("beepSound")]
    public SoundSpecifier BeepSound = new SoundPathSpecifier("/Audio/Machines/vessel_warning.ogg");
}
