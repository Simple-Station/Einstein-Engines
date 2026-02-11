using Robust.Shared.Audio;

namespace Content.Server._White.GameTicking.Aspects.Components;

[RegisterComponent]
public sealed partial class AspectComponent : Component
{
    [DataField]
    public string? Requires;

    [DataField]
    public float Weight = 1.0f;

    [DataField("forbidden")]
    public bool IsForbidden;

    [DataField("hidden")]
    public bool IsHidden;

    [DataField]
    public SoundSpecifier? StartAudio;

    [DataField]
    public SoundSpecifier? EndAudio;
}
