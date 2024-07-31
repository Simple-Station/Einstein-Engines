using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.White.Mood;

[Prototype("moodEffect")]
public sealed class MoodEffectPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("desc", required: true)]
    public string Description = string.Empty;

    [DataField("moodChange", customTypeSerializer: typeof(EnumSerializer), required: true)]
    public Enum MoodChange = default!;

    [DataField("positiveEffect", required: true)]
    public bool PositiveEffect;

    [DataField("timeout")]
    public int Timeout;

    [DataField("hidden")]
    public bool Hidden;

    //If mob already has effect of the same category, the new one will replace the old one.
    [DataField("category")]
    public string? Category;
}
