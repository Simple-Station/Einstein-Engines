using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Damage.Components
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class DamageOtherOnHitComponent : Component
    {
        [DataField, AutoNetworkedField]
        public bool IgnoreResistances = false;

        [DataField, AutoNetworkedField]
        public DamageSpecifier? Damage = null;

        [DataField, AutoNetworkedField]
        public float StaminaCost = 3.5f;

        [DataField, AutoNetworkedField]
        public int MaxHitQuantity = 1;

        [DataField, AutoNetworkedField]
        public int HitQuantity = 0;

        /// <summary>
        ///   If true, inherits the weapon damage, and sound effects from MeleeWeaponComponent,
        ///   if the respective fields are null.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool InheritMeleeStats = true;

        [DataField, AutoNetworkedField]
        public SoundSpecifier? SoundHit;

        /// <summary>
        ///     Damage done by this item when deactivated.
        /// </summary>
        public DamageSpecifier? DeactivatedDamage = null;

        /// <summary>
        ///     The noise this item makes when hitting something with it off.
        /// </summary>
        public SoundSpecifier? DeactivatedSoundHit;

        /// <summary>
        ///     The noise this item makes when hitting something with it off and it does no damage.
        /// </summary>
        public SoundSpecifier? DeactivatedSoundNoDamage;

        /// <summary>
        ///   Plays if no damage is done to the target entity.
        /// </summary>
        [DataField, AutoNetworkedField]
        public SoundSpecifier SoundNoDamage { get; set; } = new SoundCollectionSpecifier("WeakHit");
    }
}
