using Content.Shared.Damage.Systems;
using Content.Shared.Damage;

namespace Content.Shared.Damage.Components
{
    [Access(typeof(DamageOtherOnHitSystem))]
    [RegisterComponent]
    public sealed partial class DamageOtherOnHitComponent : Component
    {
        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IgnoreResistances = false;

        [DataField(required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier Damage = default!;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public float StaminaCost = 3.5f;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public int MaxHitQuantity = 1;

        [ViewVariables(VVAccess.ReadWrite)]
        public int HitQuantity = 0;
    }
}
