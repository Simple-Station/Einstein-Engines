using Content.Shared.Customization.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Bark;

[Prototype]
public sealed class BarkVoicePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public SoundSpecifier BarkSound { get; set; } = default!;

    [DataField]
    public BarkClampData ClampData { get; set; } = new();
}

[Prototype]
public sealed class BarkListPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Dictionary<ProtoId<BarkVoicePrototype>, List<CharacterRequirement>> VoiceList { get; set; } = [];
}
