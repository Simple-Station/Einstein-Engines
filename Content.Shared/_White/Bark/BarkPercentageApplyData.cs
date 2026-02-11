using Robust.Shared.Serialization;

namespace Content.Shared._White.Bark;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class BarkPercentageApplyData
{
    public static BarkPercentageApplyData Default => new();

    [DataField]
    public byte Pause { get; set; } = byte.MaxValue / 2;

    [DataField]
    public byte Volume { get; set; } = byte.MaxValue / 2;

    [DataField]
    public byte Pitch { get; set; } = byte.MaxValue / 2;

    [DataField]
    public byte PitchVariance { get; set; } = byte.MaxValue / 2;

    public BarkPercentageApplyData Clone()
    {
        return new BarkPercentageApplyData()
        {
            Pause = Pause,
            Volume = Volume,
            Pitch = Pitch,
            PitchVariance = PitchVariance,
        };
    }
}
