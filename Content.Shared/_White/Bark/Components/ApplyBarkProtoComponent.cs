using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Bark.Components;

[RegisterComponent]
public sealed partial class ApplyBarkProtoComponent : Component
{
    public static ProtoId<BarkVoicePrototype> DefaultVoice = SharedHumanoidAppearanceSystem.DefaultBarkVoice;

    [DataField]
    public ProtoId<BarkVoicePrototype> VoiceProto { get; set; } = DefaultVoice;

    [DataField]
    public BarkPercentageApplyData? PercentageApplyData { get; set; }
}
