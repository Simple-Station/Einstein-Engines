using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.Bible // Death to serverside components. Glory to Goobistan
{
    [RegisterComponent]
    public sealed partial class BibleComponent : Component
    {
        /// <summary>
        /// Damage that will be healed on a success
        /// </summary>
        [DataField(required: true)]
        public DamageSpecifier Damage = default!;

        /// <summary>
        /// Damage that will be dealt on a failure
        /// </summary>
        [DataField(required: true)]
        public DamageSpecifier DamageOnFail = default!;

        /// <summary>
        /// Damage that will be dealt when a non-chaplain attempts to heal
        /// </summary>
        [DataField(required: true)]
        public DamageSpecifier DamageOnUntrainedUse = default!;

        /// <summary>
        /// Chance the bible will fail to heal someone with no helmet
        /// </summary>
        [DataField]
        public float FailChance = 0.34f;

        [DataField("sizzleSound")]
        public SoundSpecifier SizzleSoundPath = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

        [DataField("healSound")]
        public SoundSpecifier HealSoundPath = new  SoundPathSpecifier("/Audio/Effects/holy.ogg");

        [DataField]
        public string LocPrefix = "bible";

        /// <summary>
        /// How much damage to deal to the entity being smitten - Goob
        /// </summary>
        [DataField]
        public DamageSpecifier SmiteDamage = new() {DamageDict = new Dictionary<string, FixedPoint2>() {{ "Holy", 25 }}}; // Ungodly

        /// <summary>
        /// How long to stun the entity being smitten - Goob
        /// </summary>
        [DataField]
        public TimeSpan SmiteStunDuration = TimeSpan.FromSeconds(8);

    }
}
