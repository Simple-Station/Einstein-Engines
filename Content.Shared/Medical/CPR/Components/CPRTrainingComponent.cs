using Robust.Shared.GameStates;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.Medical.CPR
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class CPRTrainingComponent : Component
    {
        [DataField]
        public SoundSpecifier? CPRSound = new SoundPathSpecifier("/Audio/Items/Defib/defib_safety_on.ogg");

        /// <summary>
        /// How long the doafter for CPR takes
        /// </summary>
        [DataField]
        public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(3);

        [DataField]
        public int AirlossHeal = 6;

        [DataField]
        public float CrackRibsModifier = 1f;
    }

    [Serializable, NetSerializable]
    public sealed partial class CPRDoAfterEvent : SimpleDoAfterEvent
    {

    }
}
