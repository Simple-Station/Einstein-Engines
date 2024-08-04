using Robust.Shared.Serialization;

namespace Content.Shared.Mood;

[Serializable, NetSerializable]
public enum MoodChangeLevel : byte
{
    None,
    Small,
    Medium,
    Big,
    Huge,
    Large
}

[Serializable, NetSerializable]
public sealed class MoodEffectEvent : EntityEventArgs
{
    public string EffectId;

    public MoodEffectEvent(string effectId)
    {
        EffectId = effectId;
    }
}

[Serializable, NetSerializable]
public sealed class MoodRemoveEffectEvent : EntityEventArgs
{
    public string EffectId;

    public MoodRemoveEffectEvent(string effectId)
    {
        EffectId = effectId;
    }
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

/// <summary>
///     This event is raised whenever an entity sets their mood, allowing other systems to modify the end result of mood math.
///     EG: The end result after tallying up all Moodlets comes out to 70, but a trait multiplies it by 0.8 to make it 56.
/// </summary>
[ByRefEvent]
public struct OnSetMoodEvent
{
    public float MoodChangedAmount;
    public EntityUid Receiver;

    public bool Cancelled;

    public OnSetMoodEvent(EntityUid receiver, float moodChangedAmount)
    {
        Receiver = receiver;
        MoodChangedAmount = moodChangedAmount;
    }
}

