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

        [DataField("amplification")]
        public float Amplification = 0.1f;

        [DataField("dampening")]
        public float Dampening = 0.1f;
        public bool Telepath = true;
        public bool TelepathicMute = false;
        public bool InnatePsiChecked = false;
    }
}
