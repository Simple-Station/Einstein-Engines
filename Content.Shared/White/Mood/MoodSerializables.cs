using Robust.Shared.Serialization;

namespace Content.Shared.White.Mood;

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
