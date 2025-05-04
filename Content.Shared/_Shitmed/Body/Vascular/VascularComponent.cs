using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System.Linq;

namespace Content.Shared._Shitmed.Body.Vascular;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VascularComponent : Component
{
    /// <summary>
    ///     The summarized capacity of all the hearts in the vascular system.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Capacity;

    /// <summary>
    ///     The normal volume of blood for this vascular system.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float NormalBloodVolume = 250.0f;

    /// <summary>
    ///     Dictionary of all current strains on the vascular system, mapped by their source.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, StrainData> Strains = new();

    /// <summary>
    ///     The total combined strain of the vascular system.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentStrain => Strains.Values.Sum(strain => strain.Value);

    /// <summary>
    ///     The current heart rate of the vascular system.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeartRate = 0f;

    /// <summary>
    ///     The speed at which the heart rate tries to rectify itself towards normality.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeartRateRecoveryRate = 5.0f;

    /// <summary>
    ///     The maximum heart rate of the vascular system for this entity. Anything above this will be considered a heart failure.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeartRateHighThreshold = 100.0f;

    /// <summary>
    ///     The minimum heart rate of the vascular system for this entity. Anything below this will be considered a heart failure.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeartRateLowThreshold = 60.0f;

    /// <summary>
    ///     How long has the heart been over the high threshold for?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TimeOverHigh = TimeSpan.Zero;

    /// <summary>
    ///     How long has the heart been under the low threshold for?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TimeUnderLow = TimeSpan.Zero;

    /// <summary>
    ///     How long must the heart be failing for before we inflict a heart attack?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan FailureTime = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     The string key of the base strain for the vascular system.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string BaseStrainName = "body_strain";
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class StrainData
{
    [DataField]
    public float Value;

    [DataField]
    public TimeSpan? Duration;
}