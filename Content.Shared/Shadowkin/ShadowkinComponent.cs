using Robust.Shared.GameStates;

namespace Content.Shared.Shadowkin
{
    [RegisterComponent]
    public sealed partial class ShadowkinComponent : Component
    {
        /// <summary>
        /// On MapInitEvent, will Blackeye the Shadowkin.
        /// </summary>
        [DataField]
        public bool BlackeyeSpawn = false;

        /// <summary>
        /// Set the Black-Eye Color.
        /// </summary>
        [DataField]
        public Color BlackEyeColor = Color.Black;
        public Color OldEyeColor = Color.LimeGreen;
    }
}
