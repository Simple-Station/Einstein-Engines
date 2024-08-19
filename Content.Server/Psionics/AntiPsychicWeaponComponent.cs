using Content.Shared.Damage;

namespace Content.Server.Psionics
{
    /// <summary>
    ///     A component for weapons intended to have special effects when wielded against Psionic Entities.
    /// </summary>
    [RegisterComponent]
    public sealed partial class AntiPsionicWeaponComponent : Component
    {

        [DataField(required: true)]
        public DamageModifierSet Modifiers = default!;

        [ViewVariables(VVAccess.ReadOnly)]
        public float PsychicStaminaDamage { get; } = 30f;

        /// <summary>
        ///     How long (in seconds) should this weapon temporarily disable powers.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public float DisableDuration { get; } = 10f;

        /// <summary>
        ///     The chances of this weapon temporarily disabling psionic powers.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public float DisableChance { get; } = 0.3f;

        /// <summary>
        ///     Whether or not the user of this weapon risks Punishment by the gods if they dare use it on non-Psionic Entities.
        /// </summary
        [ViewVariables(VVAccess.ReadOnly)]
        public bool Punish { get; } = true;

        /// <summary>
        ///     The odds of divine punishment per non-Psionic Entity attacked.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public float PunishChances { get; } = 0.5f;

        /// <summary>
        ///     How much Shock damage to take when Punish(ed) by the gods for using this weapon
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public int PunishSelfDamage { get; } = 20;

        /// <summary>
        ///     How long (in seconds) should the user be stunned when punished by the gods.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public float PunishStunDuration { get; } = 5f;
    }
}
