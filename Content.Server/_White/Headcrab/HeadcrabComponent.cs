using Content.Shared.Damage;

namespace Content.Server._White.Headcrab;

[Access(typeof(HeadcrabSystem))]
[RegisterComponent]
public sealed partial class HeadcrabComponent : Component
{
    [DataField]
    public float ParalyzeTime = 3f;

    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    public EntityUid EquippedOn;

    [ViewVariables]
    public float Accumulator = 0;

    [DataField]
    public float DamageFrequency = 5;
}
