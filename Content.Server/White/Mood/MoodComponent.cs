using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Content.Shared.White.Mood;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server.White.Mood;

[RegisterComponent]
public sealed class MoodComponent : Component
{
    [DataField("currentMoodLevel"), ViewVariables(VVAccess.ReadOnly)]
    public float CurrentMoodLevel;

    [DataField("currentMoodThreshold"), ViewVariables(VVAccess.ReadOnly)]
    public MoodThreshold CurrentMoodThreshold;

    [DataField("lastThreshold"), ViewVariables(VVAccess.ReadOnly)]
    public MoodThreshold LastThreshold;

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly Dictionary<string, string> CategorisedEffects = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly Dictionary<string, float> UncategorisedEffects = new();

    [DataField("slowdownSpeedModifier"), ViewVariables(VVAccess.ReadWrite)]
    public float SlowdownSpeedModifier = 0.75f;

    [DataField("increaseSpeedModifier"), ViewVariables(VVAccess.ReadWrite)]
    public float IncreaseSpeedModifier = 1.15f;

    [DataField("increaseCritThreshold"), ViewVariables(VVAccess.ReadWrite)]
    public float IncreaseCritThreshold = 1.2f;

    [DataField("decreaseCritThreshold"), ViewVariables(VVAccess.ReadWrite)]
    public float DecreaseCritThreshold = 0.9f;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 CritThresholdBeforeModify;

    [DataField("moodThresholds", customTypeSerializer: typeof(DictionarySerializer<MoodThreshold, float>))]
    public Dictionary<MoodThreshold, float> MoodThresholds = new()
    {
        { MoodThreshold.VeryVeryGood, 10.0f },
        { MoodThreshold.VeryGood, 8.0f },
        { MoodThreshold.Good, 7.0f },
        { MoodThreshold.Great, 6.0f },
        { MoodThreshold.Neutral, 5.0f },
        { MoodThreshold.NotGreat, 4.0f },
        { MoodThreshold.Bad, 3.0f },
        { MoodThreshold.VeryBad, 2.0f },
        { MoodThreshold.VeryVeryBad, 1.0f },
        { MoodThreshold.Dead, 0.0f }
    };

    [DataField("moodThresholdsAlerts", customTypeSerializer: typeof(DictionarySerializer<MoodThreshold, AlertType>))]
    public Dictionary<MoodThreshold, AlertType> MoodThresholdsAlerts = new()
    {
        { MoodThreshold.Dead, AlertType.MoodDead },
        { MoodThreshold.VeryVeryBad, AlertType.VeryVeryBad },
        { MoodThreshold.VeryBad, AlertType.VeryBad },
        { MoodThreshold.Bad, AlertType.Bad },
        { MoodThreshold.NotGreat, AlertType.NotGreat },
        { MoodThreshold.Neutral, AlertType.Neutral },
        { MoodThreshold.Great, AlertType.Great },
        { MoodThreshold.Good, AlertType.Good },
        { MoodThreshold.VeryGood, AlertType.VeryGood },
        { MoodThreshold.VeryVeryGood, AlertType.VeryVeryGood },
        { MoodThreshold.Insane, AlertType.Insane }
    };

    [DataField("moodChangeValues", customTypeSerializer: typeof(DictionarySerializer<Enum, float>))]
    public Dictionary<Enum, float> MoodChangeValues = new()
    {
        { MoodChangeLevel.None , 0.0f },
        { MoodChangeLevel.Small , 0.3f },
        { MoodChangeLevel.Medium , 0.7f },
        { MoodChangeLevel.Big , 1.0f },
        { MoodChangeLevel.Huge , 1.3f },
        { MoodChangeLevel.Large , 2f }
    };

    [DataField("healthMoodEffectsThresholds", customTypeSerializer: typeof(DictionarySerializer<string, float>))]
    public Dictionary<string, float> HealthMoodEffectsThresholds = new()
    {
        { "HealthHeavyDamage", 80f },
        { "HealthSevereDamage", 50f },
        { "HealthLightDamage", 10f },
        { "HealthNoDamage", 5f }
    };
}

[Serializable]
public enum MoodThreshold : ushort
{
    Insane = 1,
    VeryVeryBad = 2,
    VeryBad = 3,
    Bad = 4,
    NotGreat = 5,
    Neutral = 6,
    Great = 7,
    Good = 8,
    VeryGood = 9,
    VeryVeryGood = 10,
    Dead = 0
}
