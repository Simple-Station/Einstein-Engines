using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.HealthExaminable;

[RegisterComponent, Access(typeof(HealthExaminableSystem))]
public sealed partial class HealthExaminableComponent : Component
{
    // <summary>
    //     The thresholds for determining the examine text for certain amounts of damage.
    //     These are calculated as a percentage of the entity's critical threshold.
    // </summary>
    public List<FixedPoint2> Thresholds = new()
        { FixedPoint2.New(0.10), FixedPoint2.New(0.25), FixedPoint2.New(0.50), FixedPoint2.New(0.75) };

    [DataField("examinableTypes", required: true, customTypeSerializer:typeof(PrototypeIdHashSetSerializer<DamageTypePrototype>))]
    public HashSet<string> ExaminableTypes = default!;

    /// <summary>
    ///     Health examine text is automatically generated through creating loc string IDs, in the form:
    ///     `health-examine-[prefix]-[type]-[threshold]`
    ///     This part determines the prefix.
    /// </summary>
    [DataField("locPrefix")]
    public string LocPrefix = "carbon";
}
