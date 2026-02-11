using System.Diagnostics.Contracts;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Bark;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class BarkVoiceData
{
    [DataField]
    public SoundSpecifier BarkSound { get; set; }

    [ViewVariables(VVAccess.ReadWrite)]
    public float PauseAverage { get; set; } = 0.095f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float PitchAverage { get; set; } = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float PitchVariance { get; set; } = 0.1f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float VolumeAverage { get; set; } = 0f;

    [Pure]
    public static BarkVoiceData WithClampingValue(SoundSpecifier barkSound, BarkClampData clampData, BarkPercentageApplyData applyData)
    {
        var pauseDelta = clampData.PauseMax - clampData.PauseMin;
        var pitchDelta = clampData.PitchMax - clampData.PitchMin;
        var volumeDelta = clampData.VolumeMax - clampData.VolumeMin;
        var pitchVarianceDelta = clampData.PitchVarianceMax - clampData.PitchVarianceMin;

        return new()
        {
            BarkSound = barkSound,
            PauseAverage = clampData.PauseMin + pauseDelta * (applyData.Pause / (float)byte.MaxValue),
            PitchAverage = clampData.PitchMin + pitchDelta * (applyData.Pitch / (float)byte.MaxValue),
            VolumeAverage = clampData.VolumeMin + volumeDelta * (applyData.Volume / (float) byte.MaxValue),
            PitchVariance = clampData.PitchVarianceMin + pitchVarianceDelta * (applyData.PitchVariance / (float) byte.MaxValue)
        };
    }
}
