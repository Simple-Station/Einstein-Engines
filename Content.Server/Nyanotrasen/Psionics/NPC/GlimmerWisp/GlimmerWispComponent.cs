using Robust.Shared.Audio;

namespace Content.Server.Psionics.NPC.GlimmerWisp
{
    [RegisterComponent]
    public sealed partial class GlimmerWispComponent : Component
    {
        public bool IsDraining = false;

        /// <summary>
        /// The time (in seconds) that it takes to drain an entity.
        /// </summary>
        [DataField("drainDelay")]
        public float DrainDelay = 8.35f;

        [DataField("drainSound")]
        public SoundSpecifier DrainSoundPath = new SoundPathSpecifier("/Audio/Effects/clang2.ogg");

        [DataField("drainFinishSound")]
        public SoundSpecifier DrainFinishSoundPath = new SoundPathSpecifier("/Audio/Effects/guardian_inject.ogg");
        public EntityUid? DrainTarget;
        public EntityUid? DrainAudioStream;
    }
}
