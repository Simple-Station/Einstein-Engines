using Robust.Shared.Serialization;

namespace Content.Shared.Medical;
[Serializable, NetSerializable]
public sealed class PenLightUserMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? TargetEntity;
    public bool? Blind;
    public bool? Drunk;
    public bool? EyeDamage;
    public bool? Healthy;
    public bool? SeeingRainbows;

    public PenLightUserMessage(NetEntity? targetEntity, bool? blind, bool? drunk, bool? eyeDamage, bool? healthy, bool? seeingRainbows)
    {
        TargetEntity = targetEntity;
        Blind = blind;
        Drunk = drunk;
        EyeDamage = eyeDamage;
        Healthy = healthy;
        SeeingRainbows = seeingRainbows;
    }
}

