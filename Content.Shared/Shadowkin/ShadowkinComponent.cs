using Robust.Shared.GameStates;

namespace Content.Shared.Shadowkin
{
    [RegisterComponent]
    public sealed partial class ShadowkinComponent : Component
    {
        [DataField]
        public bool SleepManaRegen = true;

        [DataField]
        public float SleepManaRegenMultiplier = 2;

        /// <summary>
        /// On MapInitEvent, will Blackeye the Shadowkin.
        /// </summary>
        [DataField]
        public bool BlackeyeSpawn = false;

        /// <summary>
        /// If mana is equal or lower then this value, blackeye the shadowkin.
        /// </summary>
        [DataField]
        public float BlackEyeMana = 0;

        /// <summary>
        /// Set the Black-Eye Color.
        /// </summary>
        [DataField]
        public Color BlackEyeColor = Color.Black;

        public Color OldEyeColor = Color.LimeGreen;

        [DataField]
        public EntityUid? ShadowkinSleepAction;
    }
}