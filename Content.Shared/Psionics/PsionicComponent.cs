using Robust.Shared.GameStates;

namespace Content.Shared.Psionics.Abilities
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class PsionicComponent : Component
    {
        public EntityUid? PsionicAbility = null;

        /// <summary>
        ///     Ifrits, revenants, etc are explicitly magical beings that shouldn't get mindbreakered.
        /// </summary>
        [DataField("removable")]
        public bool Removable = true;

        [DataField("activePowers")]
        public List<Component> ActivePowers = new();

        [DataField("psychicFeedback")]
        public List<string> PsychicFeedback = new();

        /// <summary>
        ///     An abstraction of how "Powerful" a psychic is. This is most commonly used as a multiplier on numerical outputs for psychic powers.
        /// </summary>
        /// <remarks>
        ///     For an ordinary human, this will be between 0.5 and 1.2, but may be higher for some entities.
        /// </remarks>
        [DataField]
        public float Amplification = 0.1f;

        /// <summary>
        ///     An abstraction of how much "Control" a psychic has over their powers. This is most commonly used to decrease glimmer output of powers,
        ///     or to make obvious powers less likely to be obvious.
        /// </summary>
        [DataField]
        public float Dampening = 0.1f;
        public bool Telepath = true;
        public bool TelepathicMute = false;
        public bool InnatePsiChecked = false;
    }
}
