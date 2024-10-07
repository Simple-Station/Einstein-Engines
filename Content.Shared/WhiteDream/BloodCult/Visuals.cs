using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult;

[Serializable, NetSerializable]
public enum SoulShardVisualState : byte
{
    HasMind,
    Blessed,
    Sprite,
    Glow
}

[Serializable, NetSerializable]
public enum ConstructVisualsState : byte
{
    Transforming,
    Sprite,
    Glow
}

[Serializable, NetSerializable]
public enum TimedFactoryVisuals : byte
{
    Ready,
    Layer
}

[Serializable, NetSerializable]
public enum PylonVisuals : byte
{
    Activated,
    Layer
}

[Serializable, NetSerializable]
public enum WhetstoneVisuals : byte
{
    Used,
    Layer
}

[Serializable, NetSerializable]
public enum PentagramKey
{
    Key
}
