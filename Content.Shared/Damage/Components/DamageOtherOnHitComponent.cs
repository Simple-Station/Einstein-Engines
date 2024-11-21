using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Shared.Damage.Components
{
    [RegisterComponent]
    public sealed partial class DamageOtherOnHitComponent : Component
    {
        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IgnoreResistances = false;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier? Damage = null;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public float StaminaCost = 3.5f;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public int MaxHitQuantity = 1;

        [ViewVariables(VVAccess.ReadWrite)]
        public int HitQuantity = 0;

        /// <summary>
        ///   If true, inherits the weapon damage, and sound effects from MeleeWeaponComponent,
        ///   if the respective fields are null.
        /// </summary>
        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool InheritMeleeStats = true;

        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier? SoundHit;

        /// <summary>
        ///     Damage done by this item when deactivated.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier? DeactivatedDamage = null;

        /// <summary>
        ///     The noise this item makes when hitting something with it off.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier? DeactivatedSoundHit;

        /// <summary>
        ///     The noise this item makes when hitting something with it off and it does no damage.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier? DeactivatedSoundNoDamage;

        /// <summary>
        ///   Plays if no damage is done to the target entity.
        /// </summary>
        [DataField]
        [ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier SoundNoDamage { get; set; } = new SoundCollectionSpecifier("WeakHit");
    }
}
