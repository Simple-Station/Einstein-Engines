using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.Traits.Assorted;

/// <summary>
///     This is used for the Self-Aware trait to enhance the information received from HealthExaminableSystem.
/// </summary>
[RegisterComponent]
public sealed partial class SelfAwareComponent : Component
{
    // <summary>
    //     Damage types that an entity is able to precisely analyze like a health analyzer when they examine themselves.
    // </summary>
    [DataField("analyzableTypes", required: true, customTypeSerializer:typeof(PrototypeIdHashSetSerializer<DamageTypePrototype>))]
    public HashSet<string> AnalyzableTypes = default!;

    // <summary>
    //     Damage groups that an entity is able to detect the presence of when they examine themselves.
    // </summary>
    [DataField("detectableGroups", required: true, customTypeSerializer:typeof(PrototypeIdHashSetSerializer<DamageGroupPrototype>))]
    public HashSet<string> DetectableGroups = default!;

    // <summary>
    //     The thresholds for DetectableGroups.
    // </summary>
    public List<FixedPoint2> Thresholds = new()
        { FixedPoint2.New(10), FixedPoint2.New(25), FixedPoint2.New(50), FixedPoint2.New(75) };
}
