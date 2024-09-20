using Robust.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Damage;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class HealingWordPowerComponent : Component
    {
        [DataField]
        public DoAfterId? DoAfter;

        [DataField]
        public DamageSpecifier HealingAmount = default!;

        [DataField]
        public float RotReduction;

        [DataField]
        public float UseDelay = 1f;

        [DataField]
        public AudioParams AudioParams = default!;

        [DataField]
        public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/Psionics/heartbeat_fast.ogg");
    }
}
