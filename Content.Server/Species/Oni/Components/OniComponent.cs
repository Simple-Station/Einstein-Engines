using Content.Shared.Damage;

namespace Content.Server.Species.Oni.Components
{
    [RegisterComponent]
    public sealed partial class OniComponent : Component
    {
        [DataField("modifiers", required: true)]
        public DamageModifierSet MeleeModifiers = default!;

        [DataField("stamDamageBonus")]
        public float StamDamageMultiplier = 1.25f;
    }
}
