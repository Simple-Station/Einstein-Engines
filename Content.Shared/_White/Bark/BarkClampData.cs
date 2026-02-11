namespace Content.Shared._White.Bark;

[DataDefinition]
public sealed partial class BarkClampData
{
    [DataField]
    public float PauseMin { get; set; } = 0.05f;

    [DataField]
    public float PauseMax { get; set; } = 0.1f;

    [DataField]
    public float VolumeMin { get; set; } = 0f;

    [DataField]
    public float VolumeMax { get; set; } = 0.8f;

    [DataField]
    public float PitchMin { get; set; } = 0.8f;

    [DataField]
    public float PitchMax { get; set; } = 1.2f;

    [DataField]
    public float PitchVarianceMin { get; set; } = 0f;

    [DataField]
    public float PitchVarianceMax { get; set; } = 0.2f;
}
