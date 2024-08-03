using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.Mood;

[Prototype("moodEffect")]
public sealed class MoodEffectPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public string Description = string.Empty;

    [DataField(customTypeSerializer: typeof(EnumSerializer), required: true)]
    public Enum MoodChange = default!;

    [DataField(required: true)]
    public bool PositiveEffect;

    [DataField]
    public int Timeout;

    [DataField]
    public bool Hidden;

    //If mob already has effect of the same category, the new one will replace the old one.
    [DataField]
    public string? Category;
}
