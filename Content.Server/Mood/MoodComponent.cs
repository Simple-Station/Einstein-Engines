using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server.Mood;

[RegisterComponent]
public sealed partial class MoodComponent : Component
{
    [DataField]
    public float CurrentMoodLevel;

    [DataField]
    public MoodThreshold CurrentMoodThreshold;

    [DataField]
    public MoodThreshold LastThreshold;

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly Dictionary<string, string> CategorisedEffects = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly Dictionary<string, float> UncategorisedEffects = new();

    /// <summary>
    ///     The formula for the movement speed modifier is SpeedBonusGrowth ^ (MoodLevel - MoodThreshold.Neutral).
    ///     Change this ONLY BY 0.001 AT A TIME.
    /// </summary>
    [DataField]
    public float SpeedBonusGrowth = 1.003f;

    /// <summary>
    ///     The lowest point that low morale can multiply our movement speed by. Lowering speed follows a linear curve, rather than geometric.
    /// </summary>
    [DataField]
    public float MinimumSpeedModifier = 0.75f;

    /// <summary>
    ///     The maximum amount that high morale can multiply our movement speed by. This follows a significantly slower geometric sequence.
    /// </summary>
    [DataField]
    public float MaximumSpeedModifier = 1.15f;

    [DataField]
    public float IncreaseCritThreshold = 1.2f;

    [DataField]
    public float DecreaseCritThreshold = 0.9f;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 CritThresholdBeforeModify;

    [DataField(customTypeSerializer: typeof(DictionarySerializer<MoodThreshold, float>))]
    public Dictionary<MoodThreshold, float> MoodThresholds = new()
    {
        { MoodThreshold.Perfect, 100f },
        { MoodThreshold.Exceptional, 80f },
        { MoodThreshold.Great, 70f },
        { MoodThreshold.Good, 60f },
        { MoodThreshold.Neutral, 50f },
        { MoodThreshold.Meh, 40f },
        { MoodThreshold.Bad, 30f },
        { MoodThreshold.Terrible, 20f },
        { MoodThreshold.Horrible, 10f },
        { MoodThreshold.Dead, 0f }
    };

    [DataField(customTypeSerializer: typeof(DictionarySerializer<MoodThreshold, AlertType>))]
    public Dictionary<MoodThreshold, AlertType> MoodThresholdsAlerts = new()
    {
        { MoodThreshold.Dead, AlertType.MoodDead },
        { MoodThreshold.Horrible, AlertType.Horrible },
        { MoodThreshold.Terrible, AlertType.Terrible },
        { MoodThreshold.Bad, AlertType.Bad },
        { MoodThreshold.Meh, AlertType.Meh },
        { MoodThreshold.Neutral, AlertType.Neutral },
        { MoodThreshold.Good, AlertType.Good },
        { MoodThreshold.Great, AlertType.Great },
        { MoodThreshold.Exceptional, AlertType.Exceptional },
        { MoodThreshold.Perfect, AlertType.Perfect },
        { MoodThreshold.Insane, AlertType.Insane }
    };

    /// <summary>
    ///     These thresholds represent a percentage of Crit-Threshold, 0.8 corresponding with 80%.
    /// </summary>
    [DataField(customTypeSerializer: typeof(DictionarySerializer<string, float>))]
    public Dictionary<string, float> HealthMoodEffectsThresholds = new()
    {
        { "HealthHeavyDamage", 0.8f },
        { "HealthSevereDamage", 0.5f },
        { "HealthLightDamage", 0.1f },
        { "HealthNoDamage", 0.05f }
    };
}

[Serializable]
public enum MoodThreshold : ushort
{
    Insane = 1,
    Horrible = 2,
    Terrible = 3,
    Bad = 4,
    Meh = 5,
    Neutral = 6,
    Good = 7,
    Great = 8,
    Exceptional = 9,
    Perfect = 10,
    Dead = 0
}
