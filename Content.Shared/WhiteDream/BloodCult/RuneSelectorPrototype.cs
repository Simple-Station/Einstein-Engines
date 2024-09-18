using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult;

[Prototype("runeSelector")]
public sealed class RuneSelectorPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public EntProtoId Prototype { get; }

    [DataField]
    public float BloodCost { get; } = 20f;

    [DataField]
    public float DrawTime { get; } = 4f;

    [DataField]
    public DamageSpecifier DrawDamage { get; } = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Slash", 10 },
        }
    };
}
